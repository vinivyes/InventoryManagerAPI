using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "id", "email", "first_name", "last_name", "password", "passwordDate" },
                values: new object[] { 1, "admin@inventorym.com", "Admin", "", "$2a$11$X8whmGCPeE3xY215mIaMDeo8suaQacms23dfMkgNaukosQpGPh3u.", new DateTime(2023, 4, 11, 0, 51, 3, 191, DateTimeKind.Utc).AddTicks(4859) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "id",
                keyValue: 1);
        }
    }
}
