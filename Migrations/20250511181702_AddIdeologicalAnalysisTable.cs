using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrismaNews.Migrations
{
    public partial class AddIdeologicalAnalysisTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdeologicalAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArticleId = table.Column<string>(type: "TEXT", nullable: false),
                    XAxis = table.Column<float>(type: "REAL", nullable: false),
                    YAxis = table.Column<float>(type: "REAL", nullable: false),
                    Explanation = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    ArticleTitle = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdeologicalAnalyses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdeologicalAnalyses_ArticleId",
                table: "IdeologicalAnalyses",
                column: "ArticleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdeologicalAnalyses");
        }
    }
}
