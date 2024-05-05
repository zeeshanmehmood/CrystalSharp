using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrystalSharp.Tests.Common.Migrations.OracleMigrations
{
    public partial class OracleReadModelStoreInitTestingDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SYSTEM");

            migrationBuilder.CreateTable(
                name: "CUSTOMER",
                schema: "SYSTEM",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CODE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    GLOBALUID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ENTITYSTATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATEDON = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    MODIFIEDON = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMER", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CUSTOMER",
                schema: "SYSTEM");
        }
    }
}
