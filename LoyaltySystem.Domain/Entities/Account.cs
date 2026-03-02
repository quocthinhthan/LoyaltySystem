using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltySystem.Domain.Entities
{
    public class Account
    {
        // Khóa chính là số điện thoại
        public string PhoneNumber { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        // Quan hệ 1-1 với User (Nếu cần)
        public virtual User? User { get; set; }
    }
}
