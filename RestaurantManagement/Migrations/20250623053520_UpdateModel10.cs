using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tables",
                table: "Tables");

            migrationBuilder.RenameTable(
                name: "Tables",
                newName: "DingningTables");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DingningTables",
                table: "DingningTables",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DingningTables_TableId",
                table: "Orders",
                column: "TableId",
                principalTable: "DingningTables",
                principalColumn: "TableId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_DingningTables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "DingningTables",
                principalColumn: "TableId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DingningTables_TableId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_DingningTables_TableId",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DingningTables",
                table: "DingningTables");

            migrationBuilder.RenameTable(
                name: "DingningTables",
                newName: "Tables");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tables",
                table: "Tables",
                column: "TableId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Tables_TableId",
                table: "Orders",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "TableId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Tables_TableId",
                table: "Reservations",
                column: "TableId",
                principalTable: "Tables",
                principalColumn: "TableId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
