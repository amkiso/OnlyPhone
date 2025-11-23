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

        public ActionResult Index()
        {
            var model = xl.GetHomePageData();

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

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
                model.Notifications = xl.GetUserNotifications(userId, 10);

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

        

    
