using BCrypt.Net;
using InventoryManagerAPI.Context;
using InventoryManagerAPI.Controllers;
using InventoryManagerAPI.Models;
using InventoryManagerAPI_Tests.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace InventoryManagerAPI_Tests.ControllerTests
{
    public class RoleControllerTests
    {
        /// <summary>
        /// Creates a new test instance of the InventoryContext class with an in-memory database.
        /// Sets up an initial user with an administrator role.
        /// </summary>
        /// <returns>The test instance of the InventoryContext class.</returns>
        private TestInventoryContext GetTestDbContext()
        {
            //Creates a new DB Context for testing, uses a GUID to create a unique DB name for every test.
            var options = new DbContextOptionsBuilder<InventoryContext>()
                                        .UseInMemoryDatabase(databaseName: $"InventoryManager-{Guid.NewGuid()}")
                                        .Options;

            var dbContext = new TestInventoryContext(options);

            //Creates an Admin User
            User admin = new User { id = 1, first_name = "Admin", last_name = "", email = "admin@inventorym.com", password = BCrypt.Net.BCrypt.HashPassword("Password123!@#"), passwordDate = DateTime.UtcNow, roles = new List<Role>() };
            Role adminRole = new Role { id = 1, name = "Administrator", isActive = true, allowedActions = new string[] { "*" } };
            Role inactiveRole = new Role { id = 2, name = "Inactive Role", isActive = false, allowedActions = new string[] { "*" } };

            //Saves User and Role to DB
            dbContext.Users.Add(admin);
            dbContext.Roles.Add(adminRole);
            dbContext.Roles.Add(inactiveRole);

            //Creates a RoleUser to link the User and Role
            admin.roles.Add(adminRole);

            //Saves the changes to the DB
            dbContext.SaveChanges();

            return dbContext;
        }

        /// <summary>
        /// Test if Get All returns a list with at least one role.
        /// </summary>
        [Fact]
        public void GetAll_ReturnsListOfRoles()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);

            // Act
            var result = controller.GetAll();

            // Assert
            var objectResult = Assert.IsType<ActionResult<List<object>>>(result);
            var roles = Assert.IsAssignableFrom<List<Role>>(JsonSerializer.Deserialize<List<Role>>(
                                            JsonSerializer.SerializeToUtf8Bytes(objectResult.Value)
                                        ));
            Assert.NotEmpty(roles);
        }

        /// <summary>
        /// Tests if the GetById method returns a role when a valid role ID is provided.
        /// </summary>
        [Fact]
        public void GetById_ReturnsRole_WhenIdExists()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int existingId = 1;

            // Act
            var result = controller.GetById(existingId);
            
            // Assert
            var objectResult = Assert.IsType<ActionResult<object>>(result);
            var role = Assert.IsAssignableFrom<Role>(
                                        JsonSerializer.Deserialize<Role>(
                                            JsonSerializer.SerializeToUtf8Bytes(objectResult.Value)
                                        ));
            Assert.NotNull(role);
        }

        /// <summary>
        /// Tests if the GetById method returns a NotFoundObjectResult when an invalid role ID is provided.
        /// </summary>
        [Fact]
        public void GetById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int nonExistingId = 100;

            // Act
            var result = controller.GetById(nonExistingId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests if the GetRolesByUserId method returns a list of Roles for the specified User Id.
        /// </summary>
        [Fact]
        public void GetRolesByUserId_ReturnsListOfRolesAssignedToUser()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int existingUserId = 1;

            // Act
            var result = controller.GetRolesByUserId(existingUserId);

            // Assert
            var objectResult = Assert.IsType<ActionResult<List<object>>>(result);
            var roles = Assert.IsAssignableFrom<List<Role>>(JsonSerializer.Deserialize<List<Role>>(
                                            JsonSerializer.SerializeToUtf8Bytes(objectResult.Value)
                                        ));
            Assert.NotEmpty(roles);
        }

        /// <summary>
        /// Tests if the GetRolesByUserId method returns NotFound when the specified User Id is non-existent.
        /// </summary>
        [Fact]
        public void GetRolesByUserId_ReturnsNotFound_WhenUserIdDoesNotExist()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int nonExistingUserId = 100;

            // Act
            var result = controller.GetRolesByUserId(nonExistingUserId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests if the Create method successfully creates a new user and returns the created user.
        /// </summary>
        [Fact]
        public void Create_ReturnsCreatedRole()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            var newRole = new Role
            {
                name = "Inventory Manager",
                isActive = true,
                allowedActions = new[] { "/inventory/*" },
                notAllowedActions = new[] { "/inventory/delete" }
            };

            // Act
            var result = controller.Create(newRole);

            // Assert
            Assert.IsType<ActionResult<Role>>(result);
            var createdRole = Assert.IsAssignableFrom<Role>(
                                        JsonSerializer.Deserialize<Role>(
                                            JsonSerializer.SerializeToUtf8Bytes(result.Value)
                                        ));

            Assert.Equal(newRole.name, createdRole.name);
            Assert.Equal(newRole.isActive, createdRole.isActive);
            Assert.Equal(newRole.allowedActions, createdRole.allowedActions);
            Assert.Equal(newRole.allowedActions, createdRole.allowedActions);
        }

        /// <summary>
        /// Tests if the Create method returns a BadRequestObjectResult when there is an invalid action.
        /// </summary>
        [Fact]
        public void Create_ReturnsBadRequest_WhenActionIsNotValid()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);

            foreach (string testAction in new string[]
            {
                "inventory/read",      //Invalid action by not starting with /
                "/inventory/abc",      //Invalid action by not ending with (/read | /write | /delete)
                "/inventory//read",    //Invalid action by having multiple sequential slashes
                "/invento-y/read"      //Invalid action by having non alphanumeric characters or '/' and '*'
            })
            {
                var newRole = new Role
                {
                    name = "Test Role",
                    isActive = true,
                    allowedActions = new[] { testAction }
                };

                // Act
                var result = controller.Create(newRole);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result.Result);
            }
        }

        /// <summary>
        /// Tests if the AddRoleToUser method works as expected and after executing GetRolesByUserId returns 
        /// the newly added role in the list of roles.
        /// </summary>
        [Fact]
        public void AddRoleToUser_IsReturnedOnGetRolesByUserId()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int existingUser = 1;
            int existingRole = 1;

            //Act
            var result = controller.AddRoleToUser(existingUser, existingRole);
            var result_conf = controller.GetRolesByUserId(existingUser);

            //Assert
            Assert.IsType<OkResult>(result);
            var confirmationRoles = Assert.IsAssignableFrom<List<Role>>(
                                        JsonSerializer.Deserialize<List<Role>>(
                                            JsonSerializer.SerializeToUtf8Bytes(result_conf.Value)
                                        ));

            Assert.Contains(confirmationRoles, (r => { return r.id == existingRole; }));
        }

        /// <summary>
        /// Tests if the AddRoleToUser method returns a NotFound when either the User or Role are non-existing
        /// </summary>
        [Fact]
        public void AddRoleToUser_ReturnsNotFound_WhenEitherUserOrRoleAreNonExistent()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);

            foreach (int[] ids in new int[][] {
                                                new int[] { 100, 100 },     //Both non-existent
                                                new int[] { 100, 1   },     //Role non-existent
                                                new int[] { 1  , 100 }      //User non-existent
                                              })
            {
                //Act
                var result = controller.AddRoleToUser(ids[0], ids[1]);

                //Assert
                Assert.IsType<NotFoundObjectResult>(result);
            }
        }

        /// <summary>
        /// Tests if the AddRoleToUser method returns a NotFound when either the User or Role are non-existing
        /// </summary>
        [Fact]
        public void AddRoleToUser_ReturnsBadRequest_WhenRoleIsInactive()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int existingUser = 1;
            int inactiveRole = 2; 

            //Act
            var result = controller.AddRoleToUser(existingUser, inactiveRole);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests if the Update method successfully updates an existing Role's data.
        /// </summary>
        [Fact]
        public void Update_ReturnsUpdatedRole()
        {            
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int existingId = 1;
            var updatedData = new
            {
                name = "Update Role",
                notAllowedActions = new string[]
                {
                    "/*/delete"
                }
            };

            // Act
            var updateOp = controller.Update(existingId, updatedData);
            var result = controller.GetById(existingId);

            // Assert
            Assert.IsType<NoContentResult>(updateOp);
            Assert.IsType<ActionResult<object>>(result);
            var updatedRole = Assert.IsAssignableFrom<Role>(
                                        JsonSerializer.Deserialize<Role>(
                                            JsonSerializer.SerializeToUtf8Bytes(result.Value)
                                        ));

            Assert.Equal(updatedData.name, updatedRole.name);
            Assert.Equal(updatedData.notAllowedActions, updatedRole.notAllowedActions);
        }


        /// <summary>
        /// Tests if the Update method return NotFound when trying to update an role that does not exist.
        /// </summary>
        [Fact]
        public void Update_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int nonExistingId = 100;
            var updatedData = new
            { };

            // Act
            var result = controller.Update(nonExistingId, updatedData);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Tests if the Update method return BadRequest when trying to update actions with an invalid action string.
        /// </summary>
        [Fact]
        public void Update_ReturnsBadRequest_WhenActionIsInvalid()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int existingId = 1;

            foreach (string testAction in new string[]
            {
                "inventory/read",      //Invalid action by not starting with /
                "/inventory/abc",      //Invalid action by not ending with (/read | /write | /delete)
                "/inventory//read",    //Invalid action by having multiple sequential slashes
                "/invento-y/read"      //Invalid action by having non alphanumeric characters or '/' and '*'
            })
            {
                var updateData = new
                {
                    allowedActions = new[] { testAction }
                };

                // Act
                var result = controller.Update(existingId, updateData);

                // Assert
                Assert.IsType<BadRequestObjectResult>(result);
            }
        }


        /// <summary>
        /// Test if GetById returns NotFound when role is deleted.
        /// </summary>
        [Fact]
        public void Delete_GetById_ReturnsNotFound_WhenRoleIsDeleted()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            int existingId = 1;

            // Act
            var deleteOp = controller.Delete(existingId);
            var result = controller.GetById(existingId);

            // Assert
            Assert.IsType<OkResult>(deleteOp);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }


        /// <summary>
        /// Tests if the after running RemoveRoleFromUser, GetRolesByUserId method does not include the removed role
        /// </summary>
        [Fact]
        public void RemoveRoleFromUser_ReturnsListOfRolesWithoutRole()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);
            int existingUser = 1;
            int existingRole = 1;

            //Act
            var result = controller.RemoveRoleFromUser(existingUser, existingRole);
            var result_conf = controller.GetRolesByUserId(existingUser);

            //Assert
            Assert.IsType<NoContentResult>(result);
            var confirmationRoles = Assert.IsAssignableFrom<List<Role>>(
                                        JsonSerializer.Deserialize<List<Role>>(
                                            JsonSerializer.SerializeToUtf8Bytes(result_conf.Value)
                                        ));

            Assert.DoesNotContain(confirmationRoles, (r => { return r.id == existingRole; }));
        }


        /// <summary>
        /// Tests if the RemoveRoleFromUser method returns a NotFound when either the User is non-existent or user does not have Role
        /// </summary>
        [Fact]
        public void RemoveRoleFromUser_ReturnsNotFound_WhenEitherUserOrRoleAreNonExistent()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new RoleController(dbContext);

            foreach (int[] ids in new int[][] {
                                                new int[] { 100, 1   },     //User non-existent
                                                new int[] { 1  , 2   },     //User does not have Role
                                              })
            {
                //Act
                var result = controller.RemoveRoleFromUser(ids[0], ids[1]);

                //Assert
                Assert.IsType<NotFoundObjectResult>(result);
            }
        }
    }
}
