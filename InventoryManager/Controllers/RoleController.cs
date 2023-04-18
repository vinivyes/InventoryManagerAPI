using InventoryManagerAPI.Authorization;
using InventoryManagerAPI.Context;
using InventoryManagerAPI.Helpers;
using InventoryManagerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Text.Json;

namespace InventoryManagerAPI.Controllers
{

    /// <summary>
    /// Controller for Role routes
    /// 
    /// Available Routes:
    /// [GET] /role/                               | Gets a list of all stored Roles
    /// [GET] /role/{id}                           | Gets a Role with the specified {id}
    /// [GET] /user/{id}/role/                     | Get a list of all Roles associated with the specified User by {id}
    /// [POST] /role/                              | Create a new Role
    /// [POST] /user/{id}/role/                    | Adds a new Role to the specified User by {id}
    /// [PUT] /role/{id}                           | Update a Role with the specified {id}
    /// [DELETE] /role/{id}                        | Deletes a Role with the specified {id}
    /// [DELETE] /user/{userid}/role/{roleid}      | Removes a Role with the specified {roleid} associated with the specified User by {userid}
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly InventoryContext _context;

        public RoleController(InventoryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a list of all stored Roles
        /// </summary>
        /// <returns>A list of Roles</returns>
        [HttpGet]
        [AuthorizeAction("/role/read")]
        public ActionResult<List<object>> GetAll()
        {
            try
            {
                return _context.Roles
                               .Select(r => (object)new
                               {
                                   r.id,
                                   r.name,
                                   r.isActive,
                                   r.allowedActions,
                                   r.notAllowedActions
                               })
                              .ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100000",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Gets a Role by {id}
        /// </summary>
        /// <returns>A </returns>
        [HttpGet("{id}")]
        [AuthorizeAction("/role/read")]
        public ActionResult<object> GetById(int id)
        {
            try
            {
                return _context.Roles
                               .Where(r => r.id == id)
                               .Select(r => (object)new
                               {
                                   r.id,
                                   r.name,
                                   r.isActive,
                                   r.allowedActions,
                                   r.notAllowedActions
                               })
                               .FirstOrDefault();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100001",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Get a list of all Roles associated with the specified User by {id}
        /// </summary>
        /// <param name="id">The ID of the User</param>
        /// <returns>A list of Roles associated with the User</returns>
        [HttpGet("/user/{userid}/role")]
        [AuthorizeAction("/user/{userid}/role/read")]
        public ActionResult<List<object>> GetRolesByUserId(int userid)
        {
            try
            {
                User user = _context.Users.Include(u => u.roles).SingleOrDefault(u => u.id == userid);

                if (user == null)
                {
                    return NotFound(
                        new
                        {
                            code = "E100002",
                            message = "User could not be found"
                        });
                }

                return user.roles
                           .Where(r => r.isActive)
                           .Select(r => (object)new
                           {
                               r.id,
                               r.name,
                               r.allowedActions,
                               r.notAllowedActions
                           })
                           .ToList();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100003",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Create a new Role
        /// </summary>
        /// <param name="role">A Role object containing role data</param>
        /// <returns>The newly created Role</returns>
        [HttpPost]
        [AuthorizeAction("/role/write")]
        public ActionResult<Role> Create(Role role)
        {
            try
            {
                //Default Values
                role.users = new List<User>();

                _context.Roles.Add(role);
                _context.SaveChanges();

                return role;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100004",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Adds a new Role to the specified User by {id}
        /// </summary>
        /// <param name="id">The ID of the User</param>
        /// <param name="roleId">The ID of the Role to be added</param>
        /// <returns>Status code indicating the outcome of the operation</returns>
        [HttpPost("/user/{userid}/role/{roleId}")]
        [AuthorizeAction("/user/{userid}/role/write")]
        public IActionResult AddRoleToUser(int userid, int roleId)
        {
            try
            {
                User user = _context.Users.Include(u => u.roles).SingleOrDefault(u => u.id == userid);
                Role role = _context.Roles.SingleOrDefault(r => r.id == roleId);

                if (user == null || role == null)
                {
                    return NotFound(
                        new
                        {
                               code = user == null ? "E100005"                 : "E100006",
                            message = user == null ? "User could not be found" : "Role could not be found"
                        });
                }

                if(!role.isActive)
                    return BadRequest(new { 
                                code = "E100007",
                                message = "Role is not active"
                           });

                user.roles.Add(role);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100008",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Updates an existing role with the given ID by replacing the existing properties with the new ones provided in the request body.
        /// The request body should contain a dictionary of properties that need to be updated.
        /// If the request contains an invalid or non-existent property, a bad request status code is returned.
        /// If the given ID does not correspond to an existing role, a not found status code is returned.
        /// If the update operation is successful, a no content status code is returned.
        /// </summary>
        /// <param name="id">The ID of the role to be updated</param>
        /// <param name="role">A dictionary of properties that need to be updated</param>
        /// <returns>A status code indicating the outcome of the operation</returns>
        [HttpPut("{id}")]
        [AuthorizeAction("/role/write")]
        public IActionResult Update(int id, [FromBody] dynamic role)
        {
            try
            {
                // Retrieves the current state of the Role object
                Role existingRole = _context.Roles.SingleOrDefault(r => r.id == id);

                if (existingRole is null)
                    return NotFound(new
                    {
                        code = "E100009",
                        message = "Role could not be found"
                    });

                // Creates a dictionary of properties that should be updated
                Dictionary<string, object> updateDict = JsonSerializer.Deserialize<Dictionary<string, object>>(role);

                // Apply property values to existing Role object
                foreach (KeyValuePair<string, object> kvp in updateDict)
                {
                    if (kvp.Key == "id")
                        continue;

                    // Confirm if property exists
                    if (existingRole.GetType().GetProperty(kvp.Key) is null)
                        return BadRequest(new
                        {
                            code = "E100010",
                            message = String.Format("Property '{0}' does not exist.", kvp.Key)
                        });

                    // Gets the type of the property being updated - accounts for nullable properties
                    Type propertyType = existingRole.GetType().GetProperty(kvp.Key).PropertyType;
                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    //Converts the dynamic value into the correct type
                    object value = kvp.Value is null ? 
                                                null : 
                                                JsonSerializer.Deserialize(((JsonElement)kvp.Value).GetRawText(), propertyType);
                    
                    // Dynamically sets the existing Role properties based on the received by the request - Unspecified properties remain unchanged
                    existingRole
                        .GetType()
                        .GetProperty(kvp.Key)
                        .SetValue(
                            existingRole,
                            value,
                            null
                    );
                }

                _context.Roles.Update(existingRole);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100011",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Deletes a Role with the specified {id}
        /// </summary>
        /// <param name="id">The ID of the Role to be deleted</param>
        /// <returns>Status code indicating the outcome of the operation</returns>
        [HttpDelete("{id}")]
        [AuthorizeAction("/role/delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                Role role = _context.Roles
                                    .Include(r => r.users) // Include users to check if the role is being used
                                    .SingleOrDefault(r => r.id == id);

                if (role == null)
                {
                    return NotFound(new
                    {
                        code = "E100012",
                        message = "Role could not be found"
                    });
                }

                // Check if the role is being used by any user
                if (role.users != null && role.users.Count > 0)
                {
                    return Conflict(new
                    {
                        code = "E100013",
                        message = "This role cannot be deleted as it is associated with one or more users."
                    });
                }

                _context.Roles.Remove(role);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100014",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Removes a Role with the specified {roleid} associated with the specified User by {userid}
        /// </summary>
        /// <param name="userid">The ID of the User</param>
        /// <param name="roleid">The ID of the Role to be removed</param>
        /// <returns>Status code indicating the outcome of the operation</returns>
        [HttpDelete("/user/{userid}/role/{roleid}")]
        [AuthorizeAction("/user/{userid}/role/delete")]
        public IActionResult RemoveRoleFromUser(int userid, int roleid)
        {
            try
            {
                User user = _context.Users.Include(u => u.roles).SingleOrDefault(u => u.id == userid);
                Role role = user?.roles.SingleOrDefault(r => r.id == roleid);

                if (user == null || role == null)
                {
                    return NotFound(
                        new
                        {
                            code = user == null ? "E100015" : "E100016",
                            message = user == null ? "User could not be found" : "Role could not be found"
                        });
                }

                user.roles.Remove(role);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E100017",
                    message = Utils.Log(ex)
                });
            }
        }
    }
}
