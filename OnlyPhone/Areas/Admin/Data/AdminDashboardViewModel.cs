using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Areas.Admin.Data
{
    public class AdminDashboardViewModel
    {
        public int OnlineUsers { get; set; }
        public decimal TodayRevenue { get; set; }
        public double TodayRevenueChange { get; set; } // % thay đổi so với hôm qua
        public decimal MonthRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int DeliveredOrders { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        // Dữ liệu cho biểu đồ (7 ngày gần nhất)
        public List<string> ChartLabels { get; set; }
        public List<decimal> ChartRevenue { get; set; }
        public List<int> ChartOrders { get; set; }

        // Chi tiết trạng thái đơn hàng (để hiện ở các card nhỏ cuối)
        public int StatPending { get; set; }
        public int StatShipping { get; set; }
        public int StatDelivered { get; set; }
        public int StatCancelled { get; set; }
    }
}
  
