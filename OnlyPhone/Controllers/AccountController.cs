using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlyPhone.Models;

namespace OnlyPhone.Controllers
{
    public class AccountController : Controller
    {
        // Khởi tạo các đối tượng xử lý dữ liệu
        SQLDataClassesDataContext db = new SQLDataClassesDataContext();
        Xuly xl = new Xuly();
        EmailService em = new EmailService(); // Đưa lên cấp class để dùng chung

        // =====================================================
        // LOGIN & REGISTER VIEWS
        // =====================================================
        public ActionResult Login()
        {
            if (Session["UserSession"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        // =====================================================
        // LOGIC ĐĂNG NHẬP (POST)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Tìm user theo username hoặc email
                var user = db.Users.FirstOrDefault(u =>
                    u.users_name == model.Username ||
                    u.user_email == model.Username);

                if (user == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập không tồn tại");
                    return View(model);
                }

                // Kiểm tra mật khẩu (đã hash)
                if (!xl.VerifyPassword(model.Password, user.user_password))
                {
                    ModelState.AddModelError("", "Mật khẩu không chính xác");
                    return View(model);
                }

                // Lấy thông tin giỏ hàng
                var cart = xl.GetOrCreateCart(user.ID_user);

                // Đếm số lượng sản phẩm trong giỏ
                int cartItemCount = db.cart_items
                    .Where(ci => ci.cart_ID == cart.cart_ID)
                    .Sum(ci => (int?)ci.quantity) ?? 0;

                // Đếm thông báo chưa đọc
                int unreadNotifications = xl.GetUnreadNotificationCount(user.ID_user);

                // Cập nhật trạng thái Active
                var userdetails = db.User_details.FirstOrDefault(ud => ud.ID_user == user.ID_user);
                if (userdetails != null)
                {
                    userdetails.user_status = true;
                }
                user.LastActive = DateTime.Now;
                db.SubmitChanges();

                // Tạo session user
                var userSession = new UserSessionModel
                {
                    UserId = user.ID_user,
                    Username = user.users_name,
                    Email = user.user_email,
                    UserType = user.user_type,
                    CartId = cart.cart_ID,
                    CartItemCount = cartItemCount,
                    UnreadNotificationCount = unreadNotifications
                };

                // Lưu vào Session
                Session["UserSession"] = userSession;
                Session["UserID"] = user.ID_user;
                Session["Username"] = user.users_name;
                Session["Email"] = user.user_email;
                Session["UserType"] = user.user_type;
                Session["CartId"] = cart.cart_ID;
                Session["CartItemCount"] = cartItemCount;
                Session["UnreadNotifications"] = unreadNotifications;

                // Remember Me - tạo cookie
                if (model.RememberMe)
                {
                    var cookie = new System.Web.HttpCookie("UserLogin")
                    {
                        Value = user.ID_user.ToString(),
                        Expires = DateTime.Now.AddDays(30)
                    };
                    Response.Cookies.Add(cookie);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                return View(model);
            }
        }

        // =====================================================
        // LOGOUT
        // =====================================================
        public ActionResult Logout()
        {
            if (Session["UserID"] != null)
            {
                int uid = (int)Session["UserID"];
                var userdetail = db.User_details.FirstOrDefault(ud => ud.ID_user == uid);
                if (userdetail != null)
                {
                    userdetail.user_status = false;
                    db.SubmitChanges();
                }
            }

            Session.Clear();
            Session.Abandon();

            // Xóa cookie Remember Me
            if (Request.Cookies["UserLogin"] != null)
            {
                var cookie = new System.Web.HttpCookie("UserLogin")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Index", "Home");
        }

        // =====================================================
        // PROFILE & EDIT PROFILE
        // =====================================================
        [HttpGet]
        public ActionResult Profile()
        {
            try
            {
                if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

                int userId = (int)Session["UserID"];
                var model = xl.GetUserProfile(userId);

                if (model == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin người dùng";
                    return RedirectToAction("Index", "Home");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Profile: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult EditProfile()
        {
            try
            {
                if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

                int userId = (int)Session["UserID"];
                var profile = xl.GetUserProfile(userId);

                if (profile == null) return RedirectToAction("Profile");

                var model = new EditProfileRequest
                {
                    UserId = profile.UserId,
                    FullName = profile.FullName,
                    PhoneNumber = profile.PhoneNumber,
                    Province = profile.Province,
                    Ward = profile.Ward,
                    AddressDetail = profile.AddressDetail
                };

                return View(model);
            }
            catch
            {
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile(EditProfileRequest model)
        {
            try
            {
                if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

                int userId = (int)Session["UserID"];
                model.UserId = userId;

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin";
                    return View(model);
                }

                if (xl.IsPhoneExists(model.PhoneNumber, userId))
                {
                    TempData["ErrorMessage"] = "Số điện thoại đã được sử dụng";
                    return View(model);
                }

                if (model.AvatarFile != null && model.AvatarFile.ContentLength > 0)
                {
                    var uploadResult = xl.UploadAvatar(userId, model.AvatarFile, Server.MapPath("~/"));
                    if (!uploadResult.Success)
                    {
                        TempData["ErrorMessage"] = uploadResult.Message;
                        return View(model);
                    }
                }

                var result = xl.UpdateUserProfile(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công";
                    return RedirectToAction("Profile");
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(model);
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi hệ thống";
                return View(model);
            }
        }

        // =====================================================
        // UPLOAD AVATAR (AJAX)
        // =====================================================
        [HttpPost]
        public JsonResult UploadAvatar(HttpPostedFileBase avatarFile)
        {
            try
            {
                if (Session["UserID"] == null)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });

                int userId = (int)Session["UserID"];
                if (avatarFile == null || avatarFile.ContentLength == 0)
                    return Json(new { success = false, message = "Vui lòng chọn file ảnh" });

                var result = xl.UploadAvatar(userId, avatarFile, Server.MapPath("~/"));

                if (result.Success)
                {
                    return Json(new
                    {
                        success = true,
                        message = result.Message,
                        fileName = result.FileName,
                        filePath = Url.Content(result.FilePath)
                    });
                }
                return Json(new { success = false, message = result.Message });
            }
            catch
            {
                return Json(new { success = false, message = "Lỗi upload ảnh" });
            }
        }

        // =====================================================
        // CHANGE PASSWORD (LOGGED IN)
        // =====================================================
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            return View(new ChangePasswordRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordRequest model)
        {
            try
            {
                if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

                int userId = (int)Session["UserID"];
                model.UserId = userId;

                if (!ModelState.IsValid) return View(model);

                var result = xl.ChangePassword(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công";
                    return RedirectToAction("Profile");
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                    return View(model);
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Lỗi đổi mật khẩu";
                return View(model);
            }
        }

        // =====================================================
        // NEW FEATURE: FORGOT PASSWORD (QUÊN MẬT KHẨU)
        // =====================================================

        // BƯỚC 1: Trang nhập Email
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View(); // Đã sửa tên từ FogotPassword -> ForgotPassword
        }

        // Xử lý gửi OTP đặt lại mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendResetOTP(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Vui lòng nhập email" });

                // Kiểm tra email tồn tại
                var user = db.Users.FirstOrDefault(u => u.user_email == email);
                if (user == null)
                    return Json(new { success = false, message = "Email này chưa được đăng ký trong hệ thống" });

                // Tạo OTP
                string otpCode = GenerateOTP();

                // Lưu Session (Khác với Session đăng ký để tránh xung đột)
                Session["ResetPass_Email"] = email;
                Session["ResetPass_OTP"] = otpCode;
                Session["ResetPass_Time"] = DateTime.Now.AddMinutes(5);

                // Gửi Email (Cần cập nhật EmailService.cs như hướng dẫn trước)
                bool sent = em.SendResetPasswordOTP(email, otpCode);

                if (sent)
                    return Json(new { success = true, message = "Mã OTP đã được gửi", redirectUrl = Url.Action("VerifyResetOTP") });
                else
                    return Json(new { success = false, message = "Không thể gửi email. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // BƯỚC 2: Trang nhập OTP
        [HttpGet]
        public ActionResult VerifyResetOTP()
        {
            if (Session["ResetPass_Email"] == null)
                return RedirectToAction("ForgotPassword");

            ViewBag.Email = Session["ResetPass_Email"];
            return View();
        }

        // Xử lý kiểm tra OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CheckResetOTP(string otp)
        {
            var sessionOtp = Session["ResetPass_OTP"] as string;
            var expiry = Session["ResetPass_Time"] as DateTime?;

            if (string.IsNullOrEmpty(sessionOtp) || expiry == null)
                return Json(new { success = false, message = "Phiên làm việc hết hạn, vui lòng thực hiện lại" });

            if (DateTime.Now > expiry)
                return Json(new { success = false, message = "Mã OTP đã hết hạn" });

            if (otp != sessionOtp)
                return Json(new { success = false, message = "Mã OTP không chính xác" });

            // OTP đúng -> Cho phép sang bước đổi pass
            Session["ResetPass_CanChange"] = true;
            return Json(new { success = true, redirectUrl = Url.Action("ResetPassword") });
        }

        // BƯỚC 3: Trang đặt mật khẩu mới
        [HttpGet]
        public ActionResult ResetPassword()
        {
            // Bảo mật: Phải verify OTP thành công mới được vào đây
            if (Session["ResetPass_CanChange"] == null || (bool)Session["ResetPass_CanChange"] == false)
                return RedirectToAction("ForgotPassword");

            return View();
        }

        // Xử lý cập nhật mật khẩu mới vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ConfirmResetPassword(string newPassword)
        {
            try
            {
                if (Session["ResetPass_CanChange"] == null)
                    return Json(new { success = false, message = "Yêu cầu không hợp lệ" });

                string email = Session["ResetPass_Email"] as string;

                // Cần cập nhật Xuly.cs thêm hàm UpdatePasswordByEmail
                bool result = xl.UpdatePasswordByEmail(email, newPassword);

                if (result)
                {
                    // Xóa Session
                    Session.Remove("ResetPass_Email");
                    Session.Remove("ResetPass_OTP");
                    Session.Remove("ResetPass_Time");
                    Session.Remove("ResetPass_CanChange");

                    return Json(new { success = true, message = "Đổi mật khẩu thành công", redirectUrl = Url.Action("Login") });
                }
                else
                {
                    return Json(new { success = false, message = "Lỗi cập nhật mật khẩu" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // =====================================================
        // ORDER MANAGEMENT
        // =====================================================
        [HttpGet]
        public ActionResult OrderHistory(int page = 1)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");

            int userId = (int)Session["UserID"];
            var orders = xl.GetUserOrderHistory(userId, page, 10);
            ViewBag.CurrentPage = page;
            ViewBag.TotalOrders = orders.Count;

            return View(orders);
        }

        [HttpGet]
        public ActionResult OrderDetail(string id)
        {
            if (Session["UserID"] == null) return RedirectToAction("Login");
            if (string.IsNullOrEmpty(id)) return RedirectToAction("OrderHistory");

            int userId = (int)Session["UserID"];
            var model = xl.GetOrderDetail(id, userId);

            if (model == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("OrderHistory");
            }

            return View(model);
        }

        [HttpGet]
        public JsonResult GetRecentOrders()
        {
            try
            {
                if (Session["UserID"] == null)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);

                int userId = (int)Session["UserID"];
                var orders = xl.GetUserOrderHistory(userId, 1, 5);

                return Json(new
                {
                    success = true,
                    data = orders.Select(o => new {
                        orderId = o.OrderId,
                        orderDate = o.OrderDate,
                        statusName = o.StatusName,
                        totalAmount = o.TotalAmount,
                        itemCount = o.ItemCount
                    })
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Lỗi tải dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetOrderHistory(int page = 1, int pageSize = 10, string statusFilter = "")
        {
            try
            {
                if (Session["UserID"] == null)
                    return Json(new { success = false, message = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);

                int userId = (int)Session["UserID"];
                var allOrders = xl.GetUserOrderHistory(userId, 1, 1000);

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    int statusId = int.Parse(statusFilter);
                    allOrders = allOrders.Where(o => GetStatusId(o.StatusName) == statusId).ToList();
                }

                int totalOrders = allOrders.Count;
                int totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);
                var orders = allOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        orders = orders.Select(o => new {
                            orderId = o.OrderId,
                            orderDate = o.OrderDate,
                            statusId = GetStatusId(o.StatusName),
                            statusName = o.StatusName,
                            totalAmount = o.TotalAmount,
                            itemCount = o.ItemCount
                        }),
                        totalOrders = totalOrders,
                        totalPages = totalPages,
                        currentPage = page
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Lỗi tải dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CancelOrder(string orderId, string reason = "")
        {
            if (Session["UserID"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            int userId = (int)Session["UserID"];
            var result = xl.CancelOrder(orderId, userId, reason);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public JsonResult ReorderOrder(string orderId)
        {
            if (Session["UserID"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            int userId = (int)Session["UserID"];
            var result = xl.ReorderOrder(orderId, userId);
            return Json(new { success = result.Success, message = result.Message });
        }

        // =====================================================
        // REGISTER OTP (ĐĂNG KÝ MỚI)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendOTP(string email, string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
                    return Json(new { success = false, message = "Email và tên đăng nhập không được để trống" });

                if (db.Users.Any(u => u.user_email == email))
                    return Json(new { success = false, message = "Email đã được sử dụng" });

                if (db.Users.Any(u => u.users_name == username))
                    return Json(new { success = false, message = "Tên đăng nhập đã được sử dụng" });

                string otpCode = GenerateOTP();

                var otpModel = new RegisterViewModel.OTPModel
                {
                    Email = email,
                    OtpCode = otpCode,
                    ExpiryTime = DateTime.Now.AddMinutes(5),
                    Username = username
                };

                Session["RegisterOTP"] = otpModel;
                bool emailSent = em.SendOTPEmail(email, otpCode, username);

                if (emailSent)
                    return Json(new { success = true, message = "Mã OTP đã được gửi đến email" });
                else
                    return Json(new { success = false, message = "Không thể gửi email. Kiểm tra lại địa chỉ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyOTP(string email, string otpCode, string username, string password)
        {
            try
            {
                var otpModel = Session["RegisterOTP"] as RegisterViewModel.OTPModel;
                if (otpModel == null) return Json(new { success = false, message = "Phiên làm việc hết hạn" });

                if (otpModel.Email != email) return Json(new { success = false, message = "Email không khớp" });
                if (DateTime.Now > otpModel.ExpiryTime) return Json(new { success = false, message = "OTP đã hết hạn" });
                if (otpModel.OtpCode != otpCode) return Json(new { success = false, message = "OTP không đúng" });

                // Double check DB
                if (db.Users.Any(u => u.user_email == email)) return Json(new { success = false, message = "Email đã tồn tại" });

                var newUser = new User
                {
                    users_name = username,
                    user_email = email,
                    user_password = HashPassword(password),
                    user_type = "Customer"
                };

                db.Users.InsertOnSubmit(newUser);
                db.SubmitChanges();

                // Tạo giỏ hàng
                var cart = new shopping_cart
                {
                    ID_user = newUser.ID_user,
                    day_create = DateTime.Now,
                    update_at = DateTime.Now
                };
                db.shopping_carts.InsertOnSubmit(cart);
                db.SubmitChanges();

                // Thông báo chào mừng
                var notification = new Notification
                {
                    ID_user = newUser.ID_user,
                    Title = "Chào mừng đến với OnlyPhone!",
                    Message = $"Xin chào {username}, cảm ơn bạn đã đăng ký tài khoản.",
                    Type = "System",
                    IsRead = false,
                    Created_At = DateTime.Now
                };
                db.Notifications.InsertOnSubmit(notification);
                db.SubmitChanges();

                // Gửi mail chào mừng
                em.SendWelcomeEmail(email, username);
                Session.Remove("RegisterOTP");

                return Json(new { success = true, message = "Đăng ký thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // =====================================================
        // OTHER ACTIONS
        // =====================================================
        public ActionResult Info()
        {
            return View();
        }

        [HttpPost]
        public ActionResult KeepAlive()
        {
            if (Session["UserID"] != null)
            {
                try
                {
                    int userId = (int)Session["UserID"];
                    var user = db.Users.SingleOrDefault(u => u.ID_user == userId);
                    if (user != null)
                    {
                        user.LastActive = DateTime.Now;
                        db.SubmitChanges();
                    }
                }
                catch { }
            }
            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public JsonResult MarkNotificationAsRead(int notificationId)
        {
            try
            {
                if (Session["UserID"] == null) return Json(new { success = false, message = "Vui lòng đăng nhập" });
                int userId = (int)Session["UserID"];
                bool result = xl.MarkNotificationRead(userId, notificationId);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetNotificationDetail(int id)
        {
            try
            {
                if (Session["UserID"] == null) return Json(new { error = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);
                int userId = (int)Session["UserID"];

                var noti = xl.GetNotificationDetail(userId, id);
                if (noti == null) return Json(new { error = "Không tìm thấy thông báo" }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    Title = noti.Title,
                    CreatedDate = noti.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Content = noti.Message,
                    Url = noti.TargetURL
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================
        private string GenerateOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private int GetStatusId(string statusName)
        {
            if (string.IsNullOrEmpty(statusName)) return 0;
            if (statusName.Contains("Chờ") || statusName.Contains("Pending")) return 1;
            if (statusName.Contains("Đang xử lý") || statusName.Contains("Processing")) return 2;
            if (statusName.Contains("Xác nhận") || statusName.Contains("Confirmed")) return 3;
            if (statusName.Contains("Đang giao") || statusName.Contains("Shipping")) return 4;
            if (statusName.Contains("Hoàn thành") || statusName.Contains("Delivered")) return 5;
            if (statusName.Contains("Hủy") || statusName.Contains("Cancelled")) return 6;
            return 0;
        }
    }
}