using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCoreConfiguration.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_Country",
                columns: table => new
                {
                    KeyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_Country", x => x.KeyId);
                });

            migrationBuilder.CreateTable(
                name: "TBL_City",
                columns: table => new
                {
                    KeyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "varchar(25)", nullable: false),
                    FKid = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_City", x => x.KeyId);
                    table.ForeignKey(
                        name: "FK_TBL_City_TBL_Country_FKid",
                        column: x => x.FKid,
                        principalTable: "TBL_Country",
                        principalColumn: "KeyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_City_FKid",
                table: "TBL_City",
                column: "FKid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_City");

            migrationBuilder.DropTable(
                name: "TBL_Country");
        }
    }
}
