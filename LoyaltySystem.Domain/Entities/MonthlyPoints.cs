using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltySystem.Domain.Entities
{
    public class MonthlyPoints
    {
        // CustomerId, Month, Year sẽ tạo thành Composite Key ở tầng Infrastructure
        public int CustomerId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        // Tổng điểm tích lũy riêng trong tháng này
        public int MonthlyTotal { get; set; }

        public virtual User? Customer { get; set; }
    }
}
