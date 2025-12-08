using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Controllers
{
    public class CheckoutController : Controller
    {
        Xuly xl = new Xuly();
        // GET: Checkout
        [HttpGet]
        public ActionResult Index(string products)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["UserID"] == null)
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục thanh toán";
                    return RedirectToAction("Login", "Account", new { returnUrl = Request.Url.ToString() });
                }

                int userId = (int)Session["UserID"];

                // Parse product IDs
                if (string.IsNullOrEmpty(products))
                {
                    TempData["ErrorMessage"] = "Không có sản phẩm nào được chọn";
                    return RedirectToAction("Index", "Home");
                }

                var productIds = products.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => int.TryParse(s.Trim(), out int id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList();

                if (!productIds.Any())
                {
                    TempData["ErrorMessage"] = "Danh sách sản phẩm không hợp lệ";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy thông tin checkout
                var model = xl.GetCheckoutInfo(userId, productIds);

                if (!model.Items.Any())
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng";
                    return RedirectToAction("Index", "Home");
                }

                // Lưu product IDs vào TempData để dùng khi submit
                TempData["SelectedProducts"] = products;

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Checkout/Index: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải trang thanh toán";
                return RedirectToAction("Index", "Home");
            }
        }

        // =====================================================
        // POST: Checkout/ProcessOrder
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(CheckoutViewModel model, string products)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["UserID"] == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng đăng nhập để tiếp tục",
                        requireLogin = true
                    });
                }

                int userId = (int)Session["UserID"];

                // Validate ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng kiểm tra lại thông tin: " + string.Join(", ", errors)
                    });
                }

                // Parse product IDs
                var productIds = products.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => int.TryParse(s.Trim(), out int id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList();

                if (!productIds.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không có sản phẩm nào được chọn"
                    });
                }

                // Tạo checkout request
                var request = new CheckoutRequest
                {
                    ProductIds = productIds,
                    ShippingInfo = model.UserInfo,
                    VoucherId = model.SelectedVoucherId,
                    PaymentMethod = model.SelectedPaymentMethod,
                    ShippingMethod = model.SelectedShippingMethod
                };

                // Tạo đơn hàng
                var result = xl.CreateOrder(userId, request);

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        orderId = result.OrderId,
                        redirectUrl = Url.Action("Confirmation", "Checkout", new { orderId = result.OrderId })
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        errorCode = result.ErrorCode
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ProcessOrder: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xử lý đơn hàng: " + ex.Message
                });
            }
        }

        // =====================================================
        // GET: Checkout/Confirmation?orderId=ORD123
        // =====================================================
        [HttpGet]
        public ActionResult Confirmation(string orderId)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["UserID"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrEmpty(orderId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy thông tin đơn hàng
                var model = xl.GetOrderConfirmation(orderId);

                if (model == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index", "Home");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Confirmation: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin đơn hàng";
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult History()
        {
            ViewBag.Message = "Lịch sử mua hàng";

            return View();
        }
        // =====================================================
        // POST: Checkout/CalculateTotal (AJAX)
        // =====================================================
        [HttpPost]
        public JsonResult CalculateTotal(string products, int? voucherId, string shippingMethod)
        {
            try
            {
                if (Session["UserID"] == null)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                int userId = (int)Session["UserID"];

                // ... (Logic parse productIds giữ nguyên)
                var productIds = products.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => int.TryParse(s.Trim(), out int id) ? id : 0)
                    .Where(id => id > 0).ToList();

                var checkoutInfo = xl.GetCheckoutInfo(userId, productIds);
                decimal subTotal = checkoutInfo.SubTotal;
                decimal shippingFee = xl.CalculateShippingFee(shippingMethod ?? "Standard");

                // [CHANGE] Truyền thêm userId vào hàm tính giảm giá
                decimal discount = voucherId.HasValue ? xl.CalculateDiscount(voucherId.Value, subTotal, userId) : 0;

                decimal total = subTotal + shippingFee - discount;
                if (total < 0) total = 0;

                return Json(new
                {
                    success = true,
                    subTotal = subTotal,
                    shippingFee = shippingFee,
                    discount = discount,
                    total = total,
                    subTotalText = subTotal.ToString("N0") + "đ",
                    shippingFeeText = shippingFee.ToString("N0") + "đ",
                    discountText = discount.ToString("N0") + "đ",
                    totalText = total.ToString("N0") + "đ"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi tính toán" });
            }
        }

        

        // =====================================================
        // POST: Checkout/ValidateVoucher (AJAX)
        // =====================================================
        [HttpPost]
        public JsonResult ValidateVoucher(string voucherCode, decimal orderValue)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để sử dụng voucher" });
                }

                if (string.IsNullOrWhiteSpace(voucherCode))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã voucher" });
                }

                int userId = (int)Session["UserID"];

                // 1. Tìm voucher theo Code (Không phân biệt hoa thường)
                // Lưu ý: Cần truy cập qua context hoặc method get voucher từ Xuly
                // Ở đây giả sử gọi qua da trực tiếp hoặc method GetVoucherByCode nếu có, 
                // hoặc dùng lại list GetVouchers()
                var allVouchers = xl.GetVouchers(); // Hàm này bạn đã có trong Xuly.cs trả về List<Voucher>
                var voucher = allVouchers.FirstOrDefault(v => v.Code.Equals(voucherCode.Trim(), StringComparison.OrdinalIgnoreCase));

                if (voucher == null)
                {
                    return Json(new { success = false, message = "Mã voucher không tồn tại" });
                }

                // 2. Tính toán giảm giá (Logic kiểm tra điều kiện nằm trong hàm này)
                decimal discount = xl.CalculateDiscount(voucher.VoucherID, orderValue, userId);

                if (discount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Áp dụng mã thành công!",
                        voucherId = voucher.VoucherID,
                        discount = discount,
                        discountText = discount.ToString("N0") + "đ"
                    });
                }
                else
                {
                    // Nếu discount = 0 tức là không thỏa mãn điều kiện
                    return Json(new { success = false, message = "Voucher không khả dụng (Hết hạn, chưa đạt giá trị tối thiểu hoặc bạn không có quyền sử dụng)" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =====================================================
        // GET: Checkout/GetProvinces (AJAX)
        // =====================================================
        [HttpGet]
        public JsonResult GetProvinces()
        {
            try
            {
                var provinces = new List<string>
                {
                    "Hà Nội", "TP. Hồ Chí Minh", "Đà Nẵng", "Hải Phòng", "Cần Thơ",
                    "An Giang", "Bà Rịa - Vũng Tàu", "Bắc Giang", "Bắc Kạn", "Bạc Liêu",
                    "Bắc Ninh", "Bến Tre", "Bình Định", "Bình Dương", "Bình Phước",
                    "Bình Thuận", "Cà Mau", "Cao Bằng", "Đắk Lắk", "Đắk Nông",
                    "Điện Biên", "Đồng Nai", "Đồng Tháp", "Gia Lai", "Hà Giang",
                    "Hà Nam", "Hà Tĩnh", "Hải Dương", "Hậu Giang", "Hòa Bình",
                    "Hưng Yên", "Khánh Hòa", "Kiên Giang", "Kon Tum", "Lai Châu",
                    "Lâm Đồng", "Lạng Sơn", "Lào Cai", "Long An", "Nam Định",
                    "Nghệ An", "Ninh Bình", "Ninh Thuận", "Phú Thọ", "Phú Yên",
                    "Quảng Bình", "Quảng Nam", "Quảng Ngãi", "Quảng Ninh", "Quảng Trị",
                    "Sóc Trăng", "Sơn La", "Tây Ninh", "Thái Bình", "Thái Nguyên",
                    "Thanh Hóa", "Thừa Thiên Huế", "Tiền Giang", "Trà Vinh", "Tuyên Quang",
                    "Vĩnh Long", "Vĩnh Phúc", "Yên Bái"
                };

                return Json(provinces, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProvinces: {ex.Message}");
                return Json(new List<string>(), JsonRequestBehavior.AllowGet);
            }
        }

    }
}