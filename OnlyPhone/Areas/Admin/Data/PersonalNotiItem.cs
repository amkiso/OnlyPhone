using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Areas.Admin.Data
{
    public class PersonalNotiItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string FullMessage { get; set; }
        public string Type { get; set; } // Order, Voucher, System...
        public string TargetUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }

        // Thông tin người nhận (User)
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAvatar { get; set; }
        public string UserRole { get; set; } // Admin, Staff, Customer
    }

    public class PersonalNotiPageModel
    {
        public List<PersonalNotiItem> List { get; set; }

        // Thống kê Top Cards
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; } // Chưa đọc
        public int ReadCount { get; set; }   // Đã đọc
        public int TodayCount { get; set; }  // Mới hôm nay
    }
}