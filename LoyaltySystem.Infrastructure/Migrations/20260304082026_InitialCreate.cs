using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyaltySystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false, comment: "ID người dùng - Identity (auto-increment)")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Tên người dùng"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, comment: "Số điện thoại - Unique, FK đến Account"),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Customer", comment: "Vai trò: Admin, Staff, Customer"),
                    TotalPoint = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Tổng điểm tích lũy trọn đời (dùng cho ranking)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.UniqueConstraint("AK_Users_PhoneNumber", x => x.PhoneNumber);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, comment: "Số điện thoại - là Primary Key"),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Mật khẩu đã hash")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.PhoneNumber);
                    table.ForeignKey(
                        name: "FK_Accounts_Users_PhoneNumber",
                        column: x => x.PhoneNumber,
                        principalTable: "Users",
                        principalColumn: "PhoneNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyPoints",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false, comment: "ID khách hàng - FK đến bảng Users"),
                    Month = table.Column<int>(type: "int", nullable: false, comment: "Tháng (1-12)"),
                    Year = table.Column<int>(type: "int", nullable: false, comment: "Năm (VD: 2024)"),
                    MonthlyTotal = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Tổng điểm tích lũy trong tháng")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyPoints", x => new { x.CustomerId, x.Month, x.Year });
                    table.ForeignKey(
                        name: "FK_MonthlyPoints_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false, comment: "ID đơn hàng - Identity (auto-increment)")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false, comment: "ID khách hàng - FK đến bảng Users"),
                    StaffId = table.Column<int>(type: "int", nullable: false, comment: "ID nhân viên tạo đơn - FK đến bảng Users"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "Giá trị đơn hàng (VNĐ)"),
                    TimeCreate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()", comment: "Thời gian tạo đơn hàng")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_Customer_Time",
                table: "Orders",
                columns: new[] { "CustomerId", "TimeCreate" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_StaffId",
                table: "Orders",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_TimeCreate",
                table: "Orders",
                column: "TimeCreate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "MonthlyPoints");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
