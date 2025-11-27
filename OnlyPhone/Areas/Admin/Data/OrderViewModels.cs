using System;
using System.Collections.Generic;

namespace OnlyPhone.Areas.Admin.Data 
{
    public class OrderListViewModel
    {
        public string OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }

        public DateTime OrderDate { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string VoucherCode { get; set; }

        // Danh sách sản phẩm
        public List<OrderItemViewModel> Items { get; set; }

        public OrderListViewModel()
        {
            Items = new List<OrderItemViewModel>();
        }
    }

    public class OrderItemViewModel
    {
        public string OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Price * Quantity;
    }
}