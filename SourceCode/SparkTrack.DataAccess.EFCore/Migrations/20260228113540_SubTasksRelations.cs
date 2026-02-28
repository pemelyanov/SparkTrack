using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparkTrack.DataAccess.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class SubTasksRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubTaskDataSubTaskData",
                columns: table => new
                {
                    DependentForListId = table.Column<Guid>(type: "uuid", nullable: false),
                    DependsOnListId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubTaskDataSubTaskData", x => new { x.DependentForListId, x.DependsOnListId });
                    table.ForeignKey(
                        name: "FK_SubTaskDataSubTaskData_SubTasks_DependentForListId",
                        column: x => x.DependentForListId,
                        principalTable: "SubTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubTaskDataSubTaskData_SubTasks_DependsOnListId",
                        column: x => x.DependsOnListId,
                        principalTable: "SubTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubTaskDataSubTaskData_DependsOnListId",
                table: "SubTaskDataSubTaskData",
                column: "DependsOnListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubTaskDataSubTaskData");
        }
    }
}
