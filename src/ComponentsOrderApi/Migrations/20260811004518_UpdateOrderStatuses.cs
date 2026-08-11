using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComponentsOrderApi.Migrations
{
    public partial class UpdateOrderStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old status:
            // Done = 3
            //
            // New statuses:
            // Taken = 3
            // Returned = 4
            //
            // Convert existing completed orders from Done to Returned.

            migrationBuilder.Sql(
                "UPDATE Orders SET Status = 4 WHERE Status = 3");

            // Also update existing status-history records.
            migrationBuilder.Sql(
                "UPDATE OrderStatusHistory SET FromStatus = 4 WHERE FromStatus = 3");

            migrationBuilder.Sql(
                "UPDATE OrderStatusHistory SET ToStatus = 4 WHERE ToStatus = 3");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the conversion if the migration is rolled back.
            migrationBuilder.Sql(
                "UPDATE Orders SET Status = 3 WHERE Status = 4");

            migrationBuilder.Sql(
                "UPDATE OrderStatusHistory SET FromStatus = 3 WHERE FromStatus = 4");

            migrationBuilder.Sql(
                "UPDATE OrderStatusHistory SET ToStatus = 3 WHERE ToStatus = 4");
        }
    }
}