using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Models
{
    public class UserSessionModel
    {
        
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string UserType { get; set; }
        public int CartId { get; set; }
        public int CartItemCount { get; set; }
        public int UnreadNotificationCount { get; set; }
    }
}