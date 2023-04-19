using InventoryManagerAPI.Context;
using InventoryManagerAPI.Controllers;
using InventoryManagerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagerAPI_Tests.ControllerTests
{
    public class UserControllerTests
    {
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
            var users = Assert.IsAssignableFrom<List<object>>(objectResult.Value);
            Assert.Single(users);
        }

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
            Assert.NotNull(objectResult.Value);
        }

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

        // Add more tests for other UserController methods
    }
}
