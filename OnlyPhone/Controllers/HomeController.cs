using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Controllers
{
    public class HomeController : Controller
    {
        SQLDataClassesDataContext db = new SQLDataClassesDataContext();
     
        Xuly xl = new Xuly();

        // GET: Home/Index
        [HttpPost]
        public JsonResult SendFeedback(FeedbackViewModel model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Message))
            {
                return Json(new { success = false, message = "Vui lòng nhập đủ thông tin." });
            }

            // Gọi EmailService để gửi mail
            EmailService service = new EmailService();
            bool result = service.SendFeedbackToAdmin(model.Name, model.Email, model.Subject, model.Message);

            if (result)
            {
                return Json(new { success = true, message = "Cảm ơn bạn! Ý kiến đã được gửi đến quản trị viên." });
            }
            else
            {
                return Json(new { success = false, message = "Lỗi gửi mail. Vui lòng thử lại sau." });
            }
        }
        public ActionResult Index()
        {
            int? userId = null;
            if (Session["UserID"] != null)
            {
                userId = Convert.ToInt32(Session["UserID"]);
            }
            var model = xl.GetHomePageData();

            if (userId.HasValue)
            {
                model.Notifications = xl.GetUserNotifications(userId.Value, 10);
                model.UnreadNotificationCount = xl.GetUnreadNotificationCount(userId.Value);
            }

            return View(model);
        }

        public ActionResult About()
        {
            

            return View();
        }
       
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [ChildActionOnly]
        public ActionResult HeaderParaticalView()
        {
            var model = new UserDataViewModel();

            // Kiểm tra người dùng đã đăng nhập chưa
            if (Session["UserID"] != null)
            {
                model.IsLoggedIn = true;
                int userId = Convert.ToInt32(Session["UserID"]);
                model.UserRole = Session["UserType"]?.ToString()?.ToLower() ?? "customer";

                // Lấy số lượng thông báo chưa đọc
                model.NotificationCount = xl.GetUnreadNotificationCount(userId);

                // Lấy danh sách thông báo (10 thông báo gần nhất)
               var notis =xl.GetUserNotifications(userId, 10);
                model.Notifications = notis.Select(n => new NotificationItem
                {
                    Id = n.NotificationId, // ID này có thể là số âm (Global)
                    Title = n.Title,
                    Content = n.Message,
                    IsRead = n.IsRead,
                    CreatedDate = n.CreatedAt,
                    Type = n.Type,
                    RelatedId = n.RelatedId
                }).ToList();

                // Lấy số lượng sản phẩm trong giỏ hàng
                model.CartCount = xl.GetCartItemCount(userId);

                // Lấy danh sách sản phẩm trong giỏ hàng
                model.CartItems = xl.GetCartItems(userId);
            }
            else
            {
                model.IsLoggedIn = false;
                model.CartCount = 0;
                model.NotificationCount = 0;
            }

            return PartialView("~/Views/Shared/HeaderParaticalView.cshtml", model);
        }
        // Thêm method này vào HomeController.cs của bạn

        [HttpGet]
        public JsonResult GetCartCount()
        {
            try
            {
                int count = 0;

                if (Session["UserID"] != null)
                {
                    int userId = Convert.ToInt32(Session["UserID"]);
                    count = xl.GetCartItemCount(userId);
                }

                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { count = 0, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
       

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}

        

    
