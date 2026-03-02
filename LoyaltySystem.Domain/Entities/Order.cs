using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltySystem.Domain.Entities
{
    public class Order
    {
        public int OrderId { get; set; }

        public int CustomerId { get; set; } // FK tới User (Khách hàng)

        // Lưu ý: Đã sửa từ float thành int để khớp với UserId
        public int StaffId { get; set; }

        // Sử dụng decimal cho tiền tệ để tránh sai số dấu phẩy động
        public decimal Price { get; set; }

        public DateTime TimeCreate { get; set; } = DateTime.Now;

        public virtual User? Customer { get; set; }
    }
}
