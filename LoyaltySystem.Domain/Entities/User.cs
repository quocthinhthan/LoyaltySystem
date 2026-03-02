using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltySystem.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty; // Khóa ngoại tới Account

        public string Role { get; set; } = string.Empty; // "Admin", "Staff", "Customer"

        // Điểm tổng trọn đời giúp truy xuất ranking nhanh mà không cần SUM
        public int TotalPoint { get; set; }

        // Quan hệ với các đơn hàng
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual Account? Account { get; set; }
    }
}
