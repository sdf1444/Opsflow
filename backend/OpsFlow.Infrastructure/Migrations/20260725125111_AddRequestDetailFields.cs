using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpsFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestDetailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestComments_Users_UserId",
                table: "RequestComments");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "AuditLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AuditLogs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "AuditLogs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestComments_Users_UserId",
                table: "RequestComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestComments_Users_UserId",
                table: "RequestComments");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "AuditLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestComments_Users_UserId",
                table: "RequestComments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
