using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Models
{
    public class NotificationInfo
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string RelatedId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // Thêm các trường mới hỗ trợ Global Notification & Update DB
        public DateTime? ReadAt { get; set; }
        public string TargetURL { get; set; }
        public bool IsGlobal { get; set; } // True: Global, False: Personal
    }
}