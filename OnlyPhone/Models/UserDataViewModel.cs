using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Models
{
    public class UserDataViewModel
    {
       
        
            public bool IsLoggedIn { get; set; }
            public string UserName { get; set; }
            public string UserRole { get; set; }
            public int CartCount { get; set; }
            public int NotificationCount { get; set; }
        public List<NotificationItem> Notifications { get; set; }

        // Danh sách sản phẩm trong giỏ hàng
        public List<CartItem> CartItems { get; set; }

        public UserDataViewModel()
        {
            Notifications = new List<NotificationItem>();
            CartItems = new List<CartItem>();
        }
    }

    public class NotificationItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
        public string RelatedId { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CartItemId { get; set; }
    }
}
