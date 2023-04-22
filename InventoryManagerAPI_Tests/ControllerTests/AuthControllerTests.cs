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

namespace InventoryManagerAPI_Tests.ControllerTests
{
    public class AuthControllerTests
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
        /// Test if Login returns Unauthorized when email is non-existent.
        /// </summary>
        [Fact]
        public void Login_ReturnsUnauthorized_WhenEmailIsNotFound()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var config = TestConfiguration.GetTestConfiguration();
            var controller = new AuthController(config, dbContext);

            UserLogin login = new UserLogin() { 
                email = "non_existing@email.com",
                password = "somePassword"
            };

            // Act
            var loginOp = controller.Login(login);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(loginOp.Result);
        }

        /// <summary>
        /// Test if Login returns Unauthorized when password is incorrect.
        /// </summary>
        [Fact]
        public void Login_ReturnsUnauthorized_WhenPasswordIsIncorrect()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var config = TestConfiguration.GetTestConfiguration();
            var controller = new AuthController(config, dbContext);

            UserLogin login = new UserLogin()
            {
                email = "admin@inventorym.com",
                password = "invalidPassword"
            };

            // Act
            var loginOp = controller.Login(login);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(loginOp.Result);
        }

        /// <summary>
        /// Test if Login returns Ok when provided credentials are correct.
        /// </summary>
        [Fact]
        public void Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var dbContext = GetTestDbContext();
            var config = TestConfiguration.GetTestConfiguration();
            var controller = new AuthController(config, dbContext);

            UserLogin login = new UserLogin()
            {
                email = "admin@inventorym.com",
                password = "Password123!@#"
            };

            // Act
            var loginOp = controller.Login(login);

            // Assert
            Assert.IsType<OkObjectResult>(loginOp.Result);
        }
    }
}
