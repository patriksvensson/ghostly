using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ghostly.Data.Migrations
{
    public partial class DiscussionMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder is null)
            {
                throw new ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscussion",
                table: "WorkItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder is null)
            {
                throw new ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "IsDiscussion",
                table: "WorkItems");
        }
    }
}
