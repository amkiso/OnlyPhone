using System;
using System.Collections.Generic;

namespace OnlyPhone.Models
{
    public class OrderDetailViewModel
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment Info
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        // Shipping Info
        public string RecipientName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string FullAddress { get; set; }
        public string ShippingMethod { get; set; }
        public decimal ShippingFee { get; set; }
        public string ShippingStatus { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        // Order Items
        public List<OrderItemDetail> Items { get; set; }

        // Voucher Info
        public string VoucherCode { get; set; }
        public string VoucherDescription { get; set; }
        public decimal DiscountAmount { get; set; }

        // Calculated Values
        public decimal SubTotal { get; set; }

        public OrderDetailViewModel()
        {
            Items = new List<OrderItemDetail>();
        }
    }

    public class OrderItemDetail
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}