using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RoleUser",
                columns: new[] { "rolesid", "usersid" },
                values: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "password", "passwordDate" },
                values: new object[] { "$2a$11$RbTq/mnxc0angKoXSU25C.VuNe8ceTMO6/wbHTqmHe3zUO7uhsftS", new DateTime(2023, 4, 11, 0, 58, 12, 941, DateTimeKind.Utc).AddTicks(6409) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleUser",
                keyColumns: new[] { "rolesid", "usersid" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "password", "passwordDate" },
                values: new object[] { "$2a$11$X8whmGCPeE3xY215mIaMDeo8suaQacms23dfMkgNaukosQpGPh3u.", new DateTime(2023, 4, 11, 0, 51, 3, 191, DateTimeKind.Utc).AddTicks(4859) });
        }
    }
}
