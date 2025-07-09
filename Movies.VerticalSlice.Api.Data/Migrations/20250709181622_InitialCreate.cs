using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.VerticalSlice.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty - database already exists with correct schema
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Empty - this is just a baseline migration
        }
    }
}