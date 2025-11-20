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
        /*private readonly */
        SQLDataClassesDataContext db = new SQLDataClassesDataContext();
        Xuly xl = new Xuly();
        // GET: Account
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
        public ActionResult FogotPassword()
        {
            return View();
        }
        public ActionResult Info()
        {
            return View();
        }
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

                // Kiểm tra mật khẩu (nên sử dụng BCrypt hoặc hash password)
                // Giả sử password đã được hash trong DB
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
                int unreadNotifications = db.Notifications
                    .Count(n => n.ID_user == user.ID_user && n.IsRead == false);

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
                Session["UserId"] = user.ID_user;
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

                // Redirect dựa theo role
                if (user.user_type == "Admin" || user.user_type == "Staff")
                {
                    return RedirectToAction("Dashboard", "Manage");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                return View(model);
            }
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
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
        EmailService em = new EmailService();
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SendOTP(string email, string username)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
                {
                    return Json(new { success = false, message = "Email và tên đăng nhập không được để trống" });
                }

                // Kiểm tra email đã tồn tại
                var existingEmail = db.Users.Any(u => u.user_email == email);
                if (existingEmail)
                {
                    return Json(new { success = false, message = "Email đã được sử dụng" });
                }

                // Kiểm tra username đã tồn tại
                var existingUsername = db.Users.Any(u => u.users_name == username);
                if (existingUsername)
                {
                    return Json(new { success = false, message = "Tên đăng nhập đã được sử dụng" });
                }

                // Tạo mã OTP 6 số
                string otpCode = GenerateOTP();

                // Lưu OTP vào Session với thời gian hết hạn 5 phút
                var otpModel = new RegisterViewModel.OTPModel
                {
                    Email = email,
                    OtpCode = otpCode,
                    ExpiryTime = DateTime.Now.AddMinutes(5),
                    Username = username
                };

                Session["RegisterOTP"] = otpModel;

                // Gửi email
                bool emailSent = em.SendOTPEmail(email, otpCode, username);

                if (emailSent)
                {
                    return Json(new { success = true, message = "Mã OTP đã được gửi đến email" });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể gửi email. Vui lòng kiểm tra lại địa chỉ email" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // POST: Account/VerifyOTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult VerifyOTP(string email, string otpCode, string username, string password)
        {
            try
            {
                // Lấy OTP từ Session
                var otpModel = Session["RegisterOTP"] as RegisterViewModel.OTPModel;

                if (otpModel == null)
                {
                    return Json(new { success = false, message = "Phiên làm việc đã hết hạn. Vui lòng thử lại" });
                }

                // Kiểm tra email khớp
                if (otpModel.Email != email)
                {
                    return Json(new { success = false, message = "Email không khớp" });
                }

                // Kiểm tra OTP hết hạn
                if (DateTime.Now > otpModel.ExpiryTime)
                {
                    Session.Remove("RegisterOTP");
                    return Json(new { success = false, message = "Mã OTP đã hết hạn" });
                }

                // Kiểm tra mã OTP
                if (otpModel.OtpCode != otpCode)
                {
                    return Json(new { success = false, message = "Mã OTP không đúng" });
                }

                // Kiểm tra lại email và username chưa tồn tại (double check)
                if (db.Users.Any(u => u.user_email == email))
                {
                    return Json(new { success = false, message = "Email đã được sử dụng" });
                }

                if (db.Users.Any(u => u.users_name == username))
                {
                    return Json(new { success = false, message = "Tên đăng nhập đã được sử dụng" });
                }

                // Tạo user mới
                var newUser = new User
                {
                    users_name = username,
                    user_email = email,
                    user_password = HashPassword(password), // Hash password
                    user_type = "Customer" // Mặc định là Customer
                };

                db.Users.InsertOnSubmit(newUser);
                db.SubmitChanges();

                // Tạo giỏ hàng cho user mới
                var cart = new shopping_cart
                {
                    ID_user = newUser.ID_user,
                    day_create = DateTime.Now,
                    update_at = DateTime.Now
                };
                db.shopping_carts.InsertOnSubmit(cart);
                db.SubmitChanges();

                // Tạo thông báo chào mừng
                var notification = new Notification
                {
                    ID_user = newUser.ID_user,
                    Title = "Chào mừng đến với TechStore!",
                    Message = $"Xin chào {username}, cảm ơn bạn đã đăng ký tài khoản tại TechStore. Chúc bạn có trải nghiệm mua sắm tuyệt vời!",
                    Type = "System",
                    IsRead = false,
                    Created_At = DateTime.Now
                };
                db.Notifications.InsertOnSubmit(notification);
                db.SubmitChanges();

                // Gửi email chào mừng (không bắt buộc)
                em.SendWelcomeEmail(email, username);

                // Xóa OTP khỏi Session
                Session.Remove("RegisterOTP");

                return Json(new { success = true, message = "Đăng ký thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult MarkNotificationAsRead(int notificationId)
        {
            var notification = db.Notifications.FirstOrDefault(n => n.Notification_ID == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                db.SubmitChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        // Helper: Generate OTP 6 số
        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // Helper: Hash password (sử dụng SHA256)
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

    }
}