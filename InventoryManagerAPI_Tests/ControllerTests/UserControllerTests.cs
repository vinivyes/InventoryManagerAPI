using BCrypt.Net;
using InventoryManagerAPI.Context;
using InventoryManagerAPI.Controllers;
using InventoryManagerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace InventoryManagerAPI_Tests.ControllerTests
{
    public class UserControllerTests
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

            //Saves User and Role to DB
            dbContext.Users.Add(admin);
            dbContext.Roles.Add(adminRole);

            //Creates a RoleUser to link the User and Role
            admin.roles.Add(adminRole);

            //Saves the changes to the DB
            dbContext.SaveChanges();

            return dbContext;
        }

        /// <summary>
        /// Tests if the GetAll method returns a list of users with at least one user.
        /// </summary>
        [Fact]
        public void GetAll_ReturnsListOfUsers()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);

            // Act
            var result = controller.GetAll();

            // Assert
            var objectResult = Assert.IsType<ActionResult<List<object>>>(result);
            var users = Assert.IsAssignableFrom<List<User>>(
                                        JsonSerializer.Deserialize<List<User>>(
                                            JsonSerializer.SerializeToUtf8Bytes(objectResult.Value)
                                        ));
            Assert.NotEmpty(users);
        }

        /// <summary>
        /// Tests if the GetById method returns a user when a valid user ID is provided.
        /// </summary>
        [Fact]
        public void GetById_ReturnsUser_WhenIdExists()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            int existingId = 1;

            // Act
            var result = controller.GetById(existingId);

            // Assert
            var objectResult = Assert.IsType<ActionResult<object>>(result);
            var user = Assert.IsAssignableFrom<User>(
                                        JsonSerializer.Deserialize<User>(
                                            JsonSerializer.SerializeToUtf8Bytes(objectResult.Value)
                                        ));
            Assert.NotNull(user);
        }

        /// <summary>
        /// Tests if the GetById method returns a NotFoundObjectResult when an invalid user ID is provided.
        /// </summary>
        [Fact]
        public void GetById_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            int nonExistingId = 100;

            // Act
            var result = controller.GetById(nonExistingId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests if the Create method successfully creates a new user and returns the created user.
        /// </summary>
        [Fact]
        public void Create_ReturnsCreatedUser()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            var newUser = new User
            {
                first_name = "John",
                last_name = "Doe",
                email = "john.doe@example.com",
                password = "Password123!@#"
            };

            // Act
            var result = controller.Create(newUser);

            // Assert
            Assert.IsType<ActionResult<object>>(result);
            var createdUser = Assert.IsAssignableFrom<User>(
                                        JsonSerializer.Deserialize<User>(
                                            JsonSerializer.SerializeToUtf8Bytes(result.Value)
                                        ));
            Assert.Equal(newUser.first_name, createdUser.first_name);
            Assert.Equal(newUser.last_name, createdUser.last_name);
            Assert.Equal(newUser.email, createdUser.email);
        }


        /// <summary>
        /// Tests if the Create method returns conflict when an user with the same email already exists.
        /// </summary>
        [Fact]
        public void Create_ReturnsConflict_WhenEmailExists()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            var newUser = new User
            {
                first_name = "John",
                last_name = "Doe",
                email = "admin@inventorym.com",
                password = "Password123!@#"
            };

            // Act
            var result = controller.Create(newUser);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests if the Create method returns BadRequest when attempting to create a user with an invalid password.
        /// </summary>
        [Fact]
        public void Create_ReturnsBadRequest_WhenPasswordIsNotValid()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            var newUser = new User
            {
                first_name = "John",
                last_name = "Doe",
                email = "john.doe@example.com",
                password = "invalid"
            };

            // Act
            var result = controller.Create(newUser);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /// <summary>
        /// Tests if the Update method successfully updates an existing user's data.
        /// </summary>
        [Fact]
        public void Update_ReturnsUpdatedUser()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            int existingId = 1;
            var updatedData = new
            {
                first_name = "Updated",
                last_name = "Name"
            };

            // Act
            var updateOp = controller.Update(existingId, updatedData);
            var result = controller.GetById(existingId);

            // Assert
            Assert.IsType<NoContentResult>(updateOp);
            Assert.IsType<ActionResult<object>>(result);
            var updatedUser = Assert.IsAssignableFrom<User>(
                                        JsonSerializer.Deserialize<User>(
                                            JsonSerializer.SerializeToUtf8Bytes(result.Value)
                                        ));

            Assert.Equal(updatedData.first_name, updatedUser.first_name);
            Assert.Equal(updatedData.last_name, updatedUser.last_name);
        }

        /// <summary>
        /// Tests if the Update method return BadRequest when trying to update user email's.
        /// </summary>
        [Fact]
        public void Update_ReturnsBadRequest_WhenUpdatingEmail()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            int existingId = 1;
            var updatedData = new
            {
                email = "updated.email@example.com"
            };

            // Act
            var result = controller.Update(existingId, updatedData);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Tests if the Update method return NotFound when trying to update an user that does not exist.
        /// </summary>
        [Fact]
        public void Update_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            int nonExistingId = 100;
            var updatedData = new
            {};

            // Act
            var result = controller.Update(nonExistingId, updatedData);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }


        /// <summary>
        /// Tests if the Update method return BadRequest when trying to update a read-only property.
        /// </summary>
        [Fact]
        public void Update_ReturnsBadRequest_WhenPropertyIsReadOnly()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var controller = new UserController(dbContext);
            int existingId = 1;
            var updatedData = new
            {
                password_date = DateTime.Now
            };

            // Act
            var result = controller.Update(existingId, updatedData);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Test if GetById returns NotFound when user is deleted.
        /// </summary>
        [Fact]
        public void Delete_GetById_ReturnsNotFound_WhenUserIsDeleted()
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
    }
}
