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
    }
}