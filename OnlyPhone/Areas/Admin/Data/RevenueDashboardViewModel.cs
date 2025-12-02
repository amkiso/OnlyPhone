using System;
using System.Collections.Generic;

namespace OnlyPhone.Areas.Admin.Data
{
    public class RevenueDashboardViewModel
    {
        // 1. KPI Cards
        public decimal TotalRevenue { get; set; }
        public decimal NetProfit { get; set; } // Lợi nhuận ước tính
        public int TotalOrders { get; set; }
        public decimal AOV { get; set; } // Giá trị đơn trung bình

        // So sánh với kỳ trước (Demo %)
        public double RevenueGrowth { get; set; }
        public double OrderGrowth { get; set; }

        // 2. Chart Data
        public List<string> ChartLabels { get; set; } // Ngày
        public List<decimal> ChartRevenueData { get; set; } // Doanh thu
        public List<decimal> ChartProfitData { get; set; } // Lợi nhuận

        public List<decimal> PieData { get; set; } // [Điện thoại, Phụ kiện, Dịch vụ] - Demo category

        // 3. Recent Transactions Grid
        public List<RevenueOrderItem> RecentOrders { get; set; }
    }

    public class RevenueOrderItem
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public int TotalQuantity { get; set; } // Tổng số lượng sản phẩm
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; }
        public int StatusId { get; set; }
        public string PaymentMethod { get; set; }

        // Chi tiết để xổ xuống
        public List<RevenueProductDetail> Products { get; set; }
    }

    public class RevenueProductDetail
    {
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
}