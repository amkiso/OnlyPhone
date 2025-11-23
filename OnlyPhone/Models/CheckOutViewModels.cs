using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlyPhone.Models
{
    // =====================================================
    // CHECKOUT VIEW MODEL
    // =====================================================
    public class CheckoutViewModel
    {
        // Thông tin sản phẩm
        public List<CheckoutItem> Items { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Thông tin người dùng
        public UserShippingInfo UserInfo { get; set; }

        // Voucher
        public List<VoucherInfo> AvailableVouchers { get; set; }
        public int? SelectedVoucherId { get; set; }

        // Payment methods
        public List<PaymentMethodInfo> PaymentMethods { get; set; }
        public string SelectedPaymentMethod { get; set; }

        // Shipping methods
        public List<ShippingMethodInfo> ShippingMethods { get; set; }
        public string SelectedShippingMethod { get; set; }

        public CheckoutViewModel()
        {
            Items = new List<CheckoutItem>();
            AvailableVouchers = new List<VoucherInfo>();
            PaymentMethods = new List<PaymentMethodInfo>();
            ShippingMethods = new List<ShippingMethodInfo>();
            UserInfo = new UserShippingInfo();
        }
    }

    // =====================================================
    // CHECKOUT ITEM
    // =====================================================
    public class CheckoutItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public int AvailableStock { get; set; }
    }

    // =====================================================
    // USER SHIPPING INFO
    // =====================================================
    public class UserShippingInfo
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string RecipientName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện")]
        public string District { get; set; }

        public string Ward { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string AddressDetail { get; set; }

        public string FullAddress => $"{AddressDetail}, {(string.IsNullOrEmpty(Ward) ? "" : Ward + ", ")}{District}, {Province}";

        public string Notes { get; set; }
    }

    // =====================================================
    // VOUCHER INFO
    // =====================================================
    public class VoucherInfo
    {
        public int VoucherId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderValue { get; set; }
        public DateTime EndDate { get; set; }
        public int RemainingQuantity { get; set; }

        public string DisplayText
        {
            get
            {
                if (DiscountType == "PERCENT")
                {
                    var text = $"Giảm {DiscountValue}%";
                    if (MaxDiscountAmount.HasValue)
                        text += $" (tối đa {MaxDiscountAmount.Value:N0}đ)";
                    return text;
                }
                else
                {
                    return $"Giảm {DiscountValue:N0}đ";
                }
            }
        }
    }

    // =====================================================
    // PAYMENT METHOD INFO
    // =====================================================
    public class PaymentMethodInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public bool IsAvailable { get; set; }
    }

    // =====================================================
    // SHIPPING METHOD INFO
    // =====================================================
    public class ShippingMethodInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Fee { get; set; }
        public int EstimatedDays { get; set; }
    }

    // =====================================================
    // ORDER RESULT
    // =====================================================
    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string ErrorCode { get; set; }
    }

    // =====================================================
    // CHECKOUT REQUEST
    // =====================================================
    public class CheckoutRequest
    {
        public List<int> ProductIds { get; set; }
        public UserShippingInfo ShippingInfo { get; set; }
        public int? VoucherId { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingMethod { get; set; }
    }

    // =====================================================
    // ORDER CONFIRMATION VIEW MODEL
    // =====================================================
    public class OrderConfirmationViewModel
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }

        // Shipping Info
        public string RecipientName { get; set; }
        public string PhoneNumber { get; set; }
        public string FullAddress { get; set; }
        public string ShippingMethod { get; set; }
        public decimal ShippingFee { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }

        // Order Items
        public List<CheckoutItem> Items { get; set; }

        public OrderConfirmationViewModel()
        {
            Items = new List<CheckoutItem>();
        }
    }
}