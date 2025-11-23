using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinanceFlow.Migrations
{
    /// <inheritdoc />
    public partial class FinalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReportDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TotalGoals = table.Column<int>(type: "integer", nullable: false),
                    CompletedGoals = table.Column<int>(type: "integer", nullable: false),
                    TotalTargetAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0m),
                    TotalCurrentAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0m),
                    AverageProgress = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsReports", x => x.ReportId);
                    table.CheckConstraint("CK_Reports_Goals", "\"TotalGoals\" >= \"CompletedGoals\"");
                    table.CheckConstraint("CK_Reports_Progress", "\"AverageProgress\" >= 0 AND \"AverageProgress\" <= 100");
                    table.CheckConstraint("CK_Reports_Type", "\"ReportType\" IN ('monthly', 'quarterly', 'yearly', 'custom')");
                });

            migrationBuilder.CreateTable(
                name: "GoalCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalCategories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Goals",
                columns: table => new
                {
                    GoalId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0m),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Goals", x => x.GoalId);
                    table.CheckConstraint("CK_Goals_Amounts", "\"CurrentAmount\" <= \"TargetAmount\"");
                    table.CheckConstraint("CK_Goals_CurrentAmount", "\"CurrentAmount\" >= 0");
                    table.CheckConstraint("CK_Goals_Priority", "\"Priority\" IN (1, 2, 3)");
                    table.CheckConstraint("CK_Goals_TargetAmount", "\"TargetAmount\" > 0");
                    table.ForeignKey(
                        name: "FK_Goals_GoalCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "GoalCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoalDeposits",
                columns: table => new
                {
                    DepositId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoalId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    DepositDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Comment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DepositType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "regular")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalDeposits", x => x.DepositId);
                    table.CheckConstraint("CK_GoalDeposits_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_GoalDeposits_Type", "\"DepositType\" IN ('regular', 'salary', 'freelance', 'bonus', 'other')");
                    table.ForeignKey(
                        name: "FK_GoalDeposits_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "GoalId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalDeposits_GoalId",
                table: "GoalDeposits",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_CategoryId",
                table: "Goals",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsReports");

            migrationBuilder.DropTable(
                name: "GoalDeposits");

            migrationBuilder.DropTable(
                name: "Goals");

            migrationBuilder.DropTable(
                name: "GoalCategories");
        }
    }
}
