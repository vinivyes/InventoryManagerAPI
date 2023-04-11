using InventoryManagerAPI.Context;
using InventoryManagerAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Linq;
using InventoryManagerAPI.Helpers;
using InventoryManagerAPI.Authorization;
using System.Security.Claims;
using InventoryManagerAPI.Services;
using Microsoft.AspNetCore.Hosting.Server;

namespace InventoryManagerAPI.Controllers
{
    /// <summary>
    /// Controller for User routes
    /// 
    /// Available Routes:
    /// [GET] /user/           | Gets a list of all stored Users
    /// [GET] /user/{id}       | Get a User with the specified {id}
    /// [POST] /user/          | Create a new User
    /// [PUT] /user/{id}       | Update a User with the specified {id}
    /// [DELETE] /user/{id}    | Deletes a User with the specified {id}
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        //Instantiate configuration service
        private IConfiguration _config;
        private readonly InventoryContext _context;
        private readonly UserAuthorizationService _userAuthorizationService;

        public UserController(IConfiguration config, InventoryContext context, UserAuthorizationService userAuthorizationService)
        {
            _config = config;
            _context = context;
            _userAuthorizationService = userAuthorizationService;
        }

        /// <summary>
        /// Gets a list of all stored Users
        /// If the user requesting the information has permissions to
        /// '/roles/read' then the roles will be included in the response
        /// </summary>
        /// <returns>A list of Users</returns>
        [HttpGet]
        [AuthorizeAction("/user/read")]
        public ActionResult<List<object>> GetAll()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("userId"));

                bool canReadRoles = _userAuthorizationService.UserHasActionPermissionAsync(userId, "/roles/read").Result;

                if (canReadRoles)
                {
                    //Return only relevant fields + roles
                    return _context.Users
                                   .Include(u => u.roles)
                                   .Select(u => (object)new
                                   {
                                       u.id,
                                       u.first_name,
                                       u.last_name,
                                       u.email,
                                       u.roles
                                   })
                                   .ToList();
                }
                else
                {
                    //Return only relevant fields
                    return _context.Users
                                   .Select(u => (object)new
                                   {
                                       u.id,
                                       u.first_name,
                                       u.last_name,
                                       u.email
                                   })
                                   .ToList();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E000000",
                    message = Utils.Log(ex)
                });
            }
        }


        /// <summary>
        /// Get a user based on the {id}
        /// If the user requesting the information has permissions to
        /// '/roles/read' then the roles will be included in the response
        /// </summary>
        /// <returns>A user with the specified id</returns>
        [HttpGet("{id}")]
        [AuthorizeAction("/user/read")]
        public ActionResult<object> GetById(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue("userId"));

                bool canReadRoles = _userAuthorizationService.UserHasActionPermissionAsync(userId, "/roles/read").Result;

                if (canReadRoles)
                {
                    //Return only relevant fields + roles
                    return _context.Users
                                   .Include(u => u.roles)
                                   .Where(u => u.id == id)
                                   .Select(u => (object)new
                                   {
                                       u.id,
                                       u.first_name,
                                       u.last_name,
                                       u.email,
                                       u.roles
                                   })
                                   .FirstOrDefault();
                }
                else
                {
                    //Return only relevant fields
                    return _context.Users
                                   .Where(u => u.id == id)
                                   .Select(u => (object)new
                                   {
                                       u.id,
                                       u.first_name,
                                       u.last_name,
                                       u.email
                                   })
                                   .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E000001",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Creates a new user and saves it to the database. The user object is passed in as a parameter.
        /// The function first sets default values for passwordDate and the hashes the content of password.
        /// The user object is then added to the database and changes are saved using the DbContext.
        /// A new object with selected properties of the user is returned in the response.
        /// If an exception is caught, a status code 500 is returned with an error message.
        /// </summary>
        /// <param name="user">A User object containing user data</param>
        /// <returns>An object with selected properties of the newly created user</returns>
        [HttpPost]
        [AuthorizeAction("/user/write")]
        public ActionResult<object> Create(User user)
        {
            try
            {

                //Default Values
                user.password = BCrypt.Net.BCrypt.HashPassword(user.password);
                user.passwordDate = DateTime.UtcNow;
                user.roles = new List<Role>();

                _context.Add(user);
                _context.SaveChanges();

                return new
                {
                    user.id,
                    user.first_name,
                    user.last_name,
                    user.email
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E000002",
                    message = Utils.Log(ex)
                });
            }
        }

        /// <summary>
        /// Updates an existing user with the given ID by replacing the existing properties with the new ones provided in the request body.
        /// The request body should contain a dictionary of properties that need to be updated. Email and passwordDate properties are not modifiable via this endpoint.
        /// If the password property is included, the new password is validated and hashed, and the password date is also updated.
        /// If the request contains an invalid or non-existent property, a bad request status code is returned.
        /// If the given ID does not correspond to an existing user, a not found status code is returned.
        /// If the update operation is successful, a no content status code is returned.
        /// </summary>
        /// <param name="id">The ID of the user to be updated</param>
        /// <param name="user">A dictionary of properties that need to be updated</param>
        /// <returns>A status code indicating the outcome of the operation</returns>
        [HttpPut("{id}")]
        [AuthorizeAction("/user/write")]
        public IActionResult Update(int id, [FromBody] dynamic user)
        {
            try
            {
                //Retrieves the current state of the User Object
                User existingUser = _context.Users
                                             .SingleOrDefault((u) => u.id == id);

                if (existingUser is null)
                    return NotFound();

                //Creates a dictionary of properties that should be updated.
                Dictionary<string, object> updateDict = user.ToObject<Dictionary<string, object>>();

                //Apply property values to existing Country Object
                foreach (KeyValuePair<string, object> kvp in updateDict)
                {
                    if (kvp.Key == "id")
                        continue;

                    //Cannot modify the following keys through this API Endpoint
                    if (new string[] { "email", "passwordDate" }.Contains(kvp.Key))
                        return BadRequest(String.Format("Property '{0}' is read-only", kvp.Key));

                    //Confirm if propert exists...
                    if (existingUser.GetType().GetProperty(kvp.Key) is null)
                        return BadRequest(String.Format("Property '{0}' does not exist.", kvp.Key));

                    //When updating the password, the password must be validated and hashed - the password date is also updated.
                    if (new string[] { "password" }.Contains(kvp.Key))
                    {
                        if (!existingUser.IsPasswordValid((string)kvp.Value))
                            return BadRequest(String.Format("The new password is not valid", kvp.Key));

                        existingUser.password = BCrypt.Net.BCrypt.HashPassword((string)kvp.Value);
                        existingUser.passwordDate = DateTime.UtcNow;
                        continue;
                    }

                    //Gets the type of the property being update - accounts for nullable properties
                    Type propertyType = existingUser.GetType().GetProperty(kvp.Key).PropertyType;
                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    //Dynamically sets the existing Country properties based on the received by the request - Unspecified properties remain unchanged.
                    existingUser
                        .GetType()
                        .GetProperty(kvp.Key)
                        .SetValue(
                            existingUser,
                            kvp.Value is null ? null : Convert.ChangeType(kvp.Value, propertyType),
                            null
                    );
                }

                _context.Users
                        .Update(existingUser);
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E000003",
                    message = Utils.Log(ex)
                });
            }
        }


        [HttpDelete("{id}")]
        [AuthorizeAction("/user/delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                //Check if a User with specified Id exists
                User existingUser = _context.Users
                                            .SingleOrDefault(s => s.id == id);

                if (existingUser is null)
                    return NotFound();

                _context.Remove(existingUser);
                _context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "E000004",
                    message = Utils.Log(ex)
                });
            }
        }
    }
}
