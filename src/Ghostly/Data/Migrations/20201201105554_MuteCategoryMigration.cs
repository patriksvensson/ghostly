using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ghostly.Data.Migrations
{
    public partial class MuteCategoryMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder is null)
            {
                throw new ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.AddColumn<bool>(
                name: "IncludeMuted",
                table: "Categories",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Muted",
                table: "Categories",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder is null)
            {
                throw new ArgumentNullException(nameof(migrationBuilder));
            }

            migrationBuilder.DropColumn(
                name: "IncludeMuted",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Muted",
                table: "Categories");
        }
    }
}
