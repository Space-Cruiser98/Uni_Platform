using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComponentsOrderApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatusDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalScope",
                table: "OrderStatusHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectionReason",
                table: "OrderStatusHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnCondition",
                table: "OrderStatusHistory",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalScope",
                table: "OrderStatusHistory");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "OrderStatusHistory");

            migrationBuilder.DropColumn(
                name: "ReturnCondition",
                table: "OrderStatusHistory");
        }
    }
}
