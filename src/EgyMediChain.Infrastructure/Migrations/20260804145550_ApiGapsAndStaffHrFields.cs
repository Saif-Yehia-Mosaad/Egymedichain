using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EgyMediChain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ApiGapsAndStaffHrFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "SystemUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Facility",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "SystemUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceNumber",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "SystemUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobGrade",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficialEmail",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalEmail",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Qualification",
                table: "SystemUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InspectionRequestedAt",
                table: "RegistrationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InspectionScheduledDate",
                table: "RegistrationRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InspectorNotes",
                table: "RegistrationRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "Facility",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "InsuranceNumber",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "JobGrade",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "OfficialEmail",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "PersonalEmail",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "Qualification",
                table: "SystemUsers");

            migrationBuilder.DropColumn(
                name: "InspectionRequestedAt",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "InspectionScheduledDate",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "InspectorNotes",
                table: "RegistrationRequests");
        }
    }
}
