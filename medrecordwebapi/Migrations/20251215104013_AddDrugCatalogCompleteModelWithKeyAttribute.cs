using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medrecordwebapi.Migrations
{
    /// <inheritdoc />
    public partial class AddDrugCatalogCompleteModelWithKeyAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "MedicationName",
                table: "Prescriptions");

            migrationBuilder.AlterColumn<string>(
                name: "DurationDays",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DrugId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DrugCatalogs",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Composition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DosageStrength = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugCatalogs", x => x.DrugId);
                });

            migrationBuilder.CreateTable(
                name: "TestCatalogs",
                columns: table => new
                {
                    TestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StandardRange = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCatalogs", x => x.TestId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DrugId",
                table: "Prescriptions",
                column: "DrugId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_DrugCatalogs_DrugId",
                table: "Prescriptions",
                column: "DrugId",
                principalTable: "DrugCatalogs",
                principalColumn: "DrugId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_DrugCatalogs_DrugId",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "DrugCatalogs");

            migrationBuilder.DropTable(
                name: "TestCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_DrugId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "DrugId",
                table: "Prescriptions");

            migrationBuilder.AlterColumn<int>(
                name: "DurationDays",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MedicationName",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
