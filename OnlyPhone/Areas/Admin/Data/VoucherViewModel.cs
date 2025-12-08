using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Areas.Admin.Data
{
    // ViewModel hiển thị danh sách Voucher
    public class VoucherViewModel
    {
        public int VoucherID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsPercent { get; set; }
        public decimal DiscountValue { get; set; } // Số tiền hoặc %
        public int Quantity { get; set; }
        public int QuantityUsed { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; } // Public hoặc Private
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int MaxUsagePerUser { get; set; }
        public string StatusLabel { get; set; } // "Đang chạy", "Hết hạn"...
    }

    // ViewModel thống kê
    public class VoucherStatsModel
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Expired { get; set; }
        public int Private { get; set; }
    }

    // ViewModel phân trang
    public class VoucherPageModel
    {
        public List<VoucherViewModel> List { get; set; }
        public VoucherStatsModel Stats { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string Keyword { get; set; }
        public int? StatusFilter { get; set; } // 1: Active, 2: Expired, 3: Private
    }
}