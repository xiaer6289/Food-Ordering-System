using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Migrations
{
    /// <inheritdoc />
    public partial class Orderdetailorderitempayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_OrderDetails_OrderId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "FoodOrderDetail");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Payments",
                newName: "PaymentId");

            migrationBuilder.AddColumn<string>(
                name: "OrderDetailId",
                table: "Payments",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StripeTransactionId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OrderDetails",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDetailId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FoodId = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Foods_FoodId",
                        column: x => x.FoodId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_OrderDetails_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderDetailId",
                table: "Payments",
                column: "OrderDetailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_FoodId",
                table: "OrderItems",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderDetailId",
                table: "OrderItems",
                column: "OrderDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_OrderDetails_OrderDetailId",
                table: "Payments",
                column: "OrderDetailId",
                principalTable: "OrderDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_OrderDetails_OrderDetailId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderDetailId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OrderDetailId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StripeTransactionId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "Payments",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "Payments",
                type: "nvarchar(4)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "OrderDetails",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "FoodOrderDetail",
                columns: table => new
                {
                    FoodsId = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    OrderDetailsId = table.Column<string>(type: "nvarchar(4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodOrderDetail", x => new { x.FoodsId, x.OrderDetailsId });
                    table.ForeignKey(
                        name: "FK_FoodOrderDetail_Foods_FoodsId",
                        column: x => x.FoodsId,
                        principalTable: "Foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodOrderDetail_OrderDetails_OrderDetailsId",
                        column: x => x.OrderDetailsId,
                        principalTable: "OrderDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoodOrderDetail_OrderDetailsId",
                table: "FoodOrderDetail",
                column: "OrderDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_OrderDetails_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "OrderDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
