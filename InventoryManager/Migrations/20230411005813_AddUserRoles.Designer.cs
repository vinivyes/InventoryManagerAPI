﻿// <auto-generated />
using System;
using InventoryManagerAPI.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryManagerAPI.Migrations
{
    [DbContext(typeof(InventoryContext))]
    [Migration("20230411005813_AddUserRoles")]
    partial class AddUserRoles
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("InventoryManagerAPI.Models.Role", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string[]>("allowedActions")
                        .HasColumnType("text[]");

                    b.Property<bool>("isActive")
                        .HasColumnType("boolean");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string[]>("notAllowedActions")
                        .HasColumnType("text[]");

                    b.HasKey("id");

                    b.HasIndex("name")
                        .IsUnique();

                    b.ToTable("Role");

                    b.HasData(
                        new
                        {
                            id = 1,
                            allowedActions = new[] { "*" },
                            isActive = true,
                            name = "Administrator"
                        },
                        new
                        {
                            id = 2,
                            allowedActions = new[] { "/inventory/*", "/product/*" },
                            isActive = true,
                            name = "Inventory Manager"
                        });
                });

            modelBuilder.Entity("InventoryManagerAPI.Models.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("first_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("last_name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("passwordDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("id");

                    b.HasIndex("email")
                        .IsUnique();

                    b.ToTable("User");

                    b.HasData(
                        new
                        {
                            id = 1,
                            email = "admin@inventorym.com",
                            first_name = "Admin",
                            last_name = "",
                            password = "$2a$11$RbTq/mnxc0angKoXSU25C.VuNe8ceTMO6/wbHTqmHe3zUO7uhsftS",
                            passwordDate = new DateTime(2023, 4, 11, 0, 58, 12, 941, DateTimeKind.Utc).AddTicks(6409)
                        });
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.Property<int>("rolesid")
                        .HasColumnType("integer");

                    b.Property<int>("usersid")
                        .HasColumnType("integer");

                    b.HasKey("rolesid", "usersid");

                    b.HasIndex("usersid");

                    b.ToTable("RoleUser");

                    b.HasData(
                        new
                        {
                            rolesid = 1,
                            usersid = 1
                        });
                });

            modelBuilder.Entity("RoleUser", b =>
                {
                    b.HasOne("InventoryManagerAPI.Models.Role", null)
                        .WithMany()
                        .HasForeignKey("rolesid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("InventoryManagerAPI.Models.User", null)
                        .WithMany()
                        .HasForeignKey("usersid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
