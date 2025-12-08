using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Models
{
    public class PromotionViewModel
    {
        public int VoucherId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool DiscountType { get; set; } 
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderValue { get; set; }
        public DateTime? EndDate { get; set; }

        // Trạng thái hiển thị
        public bool IsSaved { get; set; } // User đã lưu chưa?
        public int PercentUsed { get; set; } // % Đã sử dụng (để hiện thanh tiến độ)
        public bool IsOutStock { get; set; } // Hết lượt chưa
    }
}