using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparkTrack.DataAccess.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class FeatureAuthors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeatureDataUserData",
                columns: table => new
                {
                    AuthorsListId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeaturesListId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureDataUserData", x => new { x.AuthorsListId, x.FeaturesListId });
                    table.ForeignKey(
                        name: "FK_FeatureDataUserData_Features_FeaturesListId",
                        column: x => x.FeaturesListId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatureDataUserData_Users_AuthorsListId",
                        column: x => x.AuthorsListId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureDataUserData_FeaturesListId",
                table: "FeatureDataUserData",
                column: "FeaturesListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureDataUserData");
        }
    }
}
