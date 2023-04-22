
using InventoryManagerAPI.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace InventoryManagerAPI.Context
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Roles
            modelBuilder.Entity<Role>().HasData(new Role { id = 1, name = "Administrator", isActive = true, allowedActions = new string[] { "*" } });
            modelBuilder.Entity<Role>().HasData(new Role { id = 2, name = "Inventory Manager", isActive = true, allowedActions = new string[] { "/inventory/*", "/product/*", "/role/read" } });

            //Users
            modelBuilder.Entity<User>().HasData(new User { id = 1, first_name = "Admin", last_name = "", email = "admin@inventorym.com", password = BCrypt.Net.BCrypt.HashPassword("Password123!@#"), passwordDate = DateTime.UtcNow });

            // User-Roles
            modelBuilder.Entity("RoleUser").HasData(new { usersid = 1, rolesid = 1 });

            //Categories
            modelBuilder.Entity<Category>().HasData(new Category { id = 1, name = "Electronics", isActive = true, description = "Electronic Devices go in this categories, such as: Cellphones, TVs, etc." });

        }
    }
}
