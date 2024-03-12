using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMicroservices.Migrations
{
    public partial class ThridCommit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualFileUrl",
                table: "UsersAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "UsersAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "UsersAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "UsersAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ManagerEmail",
                table: "UsersAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ManagerName",
                table: "UsersAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "UsersAccounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PasswordResetToken",
                table: "UsersAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PhoneNumber",
                table: "UsersAccounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "UsersAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "UsersAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualFileUrl",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "ManagerEmail",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "ManagerName",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpires",
                table: "UsersAccounts");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "UsersAccounts");
        }
    }
}
