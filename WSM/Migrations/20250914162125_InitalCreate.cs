using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Migrations
{
    /// <inheritdoc />
    public partial class InitalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SeatNo",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.CreateTable(
                name: "Seat",
                columns: table => new
                {
                    SeatNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seat", x => x.SeatNo);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_SeatNo",
                table: "OrderDetails",
                column: "SeatNo");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Seat_SeatNo",
                table: "OrderDetails",
                column: "SeatNo",
                principalTable: "Seat",
                principalColumn: "SeatNo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Seat_SeatNo",
                table: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Seat");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_SeatNo",
                table: "OrderDetails");

            migrationBuilder.AlterColumn<string>(
                name: "SeatNo",
                table: "OrderDetails",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
