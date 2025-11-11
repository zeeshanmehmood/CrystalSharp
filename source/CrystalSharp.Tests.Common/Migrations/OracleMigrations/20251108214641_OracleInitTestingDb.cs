using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrystalSharp.Tests.Common.Migrations.OracleMigrations
{
    public partial class OracleInitTestingDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "SYSTEM");

            migrationBuilder.CreateTable(
                name: "EMPLOYEE",
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
                    MODIFIEDON = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    VERSION = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYEE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SALEORDER",
                schema: "SYSTEM",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CODE = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TOTALAMOUNT = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GLOBALUID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ENTITYSTATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATEDON = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    MODIFIEDON = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    VERSION = table.Column<long>(type: "NUMBER(19)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SALEORDER", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ORDERDETAIL",
                schema: "SYSTEM",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    SALEORDERID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NAME = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    QUANTITY = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PRICE = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GLOBALUID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    ENTITYSTATUS = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CREATEDON = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    MODIFIEDON = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDERDETAIL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ORDERDETAIL_SALEORDER_SALEORDERID",
                        column: x => x.SALEORDERID,
                        principalSchema: "SYSTEM",
                        principalTable: "SALEORDER",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ORDERDETAIL_SALEORDERID",
                schema: "SYSTEM",
                table: "ORDERDETAIL",
                column: "SALEORDERID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EMPLOYEE",
                schema: "SYSTEM");

            migrationBuilder.DropTable(
                name: "ORDERDETAIL",
                schema: "SYSTEM");

            migrationBuilder.DropTable(
                name: "SALEORDER",
                schema: "SYSTEM");
        }
    }
}
