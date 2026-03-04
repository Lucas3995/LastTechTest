using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastTechTest.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestedAtUtcToAnticipationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedAtUtc",
                table: "AnticipationRequests",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedAtUtc",
                table: "AnticipationRequests");
        }
    }
}
