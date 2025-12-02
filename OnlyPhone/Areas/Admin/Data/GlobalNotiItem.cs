using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Areas.Admin.Data
{
    public class GlobalNotiItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string FullMessage { get; set; }
        public string Type { get; set; }        // System, Event, Maintenance...
        public string TargetRole { get; set; }  // All, Customer, Staff, Admin
        public string TargetUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatorName { get; set; } // Tên Admin tạo

        // Thống kê tương tác
        public int TotalTargetUsers { get; set; } // Tổng số người nhận dự kiến
        public int ReadCount { get; set; }        // Số người đã đọc
        public int UnreadCount { get; set; }      // Số người chưa đọc
    }

    // Model hiển thị trong Popup (Chi tiết người đọc)
    public class GlobalReaderModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Avatar { get; set; }
        public bool HasRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    // Model tổng cho trang Index
    public class GlobalNotiPageModel
    {
        public List<GlobalNotiItem> List { get; set; }
        public int TotalActive { get; set; } // Đang hiệu lực
        public int TotalExpired { get; set; } // Đã hết hạn
        public int TotalReads { get; set; }   // Tổng lượt đọc toàn hệ thống
    }
}
