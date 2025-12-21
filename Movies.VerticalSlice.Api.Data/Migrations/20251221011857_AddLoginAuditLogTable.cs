using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.VerticalSlice.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAuditLogs_Email",
                table: "LoginAuditLogs",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAuditLogs_Status",
                table: "LoginAuditLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAuditLogs_Timestamp",
                table: "LoginAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAuditLogs_UserId",
                table: "LoginAuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginAuditLogs");
        }
    }
}
