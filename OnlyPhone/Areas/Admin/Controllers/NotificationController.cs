using On.Areas.Admin;
using OnlyPhone.Areas.Admin.Data;
using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Areas.Admin.Controllers
{
    [AdminSecurity]
    public class NotificationController : Controller
    {
        Xuly da = new Xuly();

        // GET: Admin/Notifications
        public ActionResult Persion(string keyword, string start, string end)
        {
            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(start, out DateTime d1)) fromDate = d1;
            if (DateTime.TryParse(end, out DateTime d2)) toDate = d2;

            ViewBag.Keyword = keyword;
            ViewBag.Start = start;
            ViewBag.End = end;

            // Lấy trang 1, 10 item
            var model = da.GetPersonalNotifications(keyword, fromDate, toDate, 1, 10);
            return View(model);
        }
        [HttpGet]
        public ActionResult PersionLoadMore(string keyword, string start, string end, int page)
        {
            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(start, out DateTime d1)) fromDate = d1;
            if (DateTime.TryParse(end, out DateTime d2)) toDate = d2;

            var model = da.GetPersonalNotifications(keyword, fromDate, toDate, page, 10);
            return Json(new { success = true, data = model.List }, JsonRequestBehavior.AllowGet);
        }
        // POST: Delete
        [HttpPost]
        public ActionResult Delete(int id)
        {
            bool result = da.DeletePersonalNotification(id);
            return Json(new { success = result, message = result ? "Đã xóa thành công" : "Lỗi hệ thống" });
        }
        public ActionResult Global(string keyword, string role, string start, string end)
        {
            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(start, out DateTime d1)) fromDate = d1;
            if (DateTime.TryParse(end, out DateTime d2)) toDate = d2;

            ViewBag.Keyword = keyword;
            ViewBag.Role = role;
            ViewBag.Start = start;
            ViewBag.End = end;

            // Page 1, Size 10
            var model = da.GetGlobalNotifications(keyword, role, fromDate, toDate, 1, 10);
            return View(model);
        }

        // API: Load More (Lấy các trang tiếp theo)
        [HttpGet]
        public ActionResult LoadMore(string keyword, string role, string start, string end, int page)
        {
            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(start, out DateTime d1)) fromDate = d1;
            if (DateTime.TryParse(end, out DateTime d2)) toDate = d2;

            var model = da.GetGlobalNotifications(keyword, role, fromDate, toDate, page, 10);
            return Json(new { success = true, data = model.List }, JsonRequestBehavior.AllowGet);
        }

        // AJAX: Lấy danh sách người đọc cho Popup
        [HttpGet]
        public ActionResult GetReaders(int id, string type)
        {
            // type: "read" hoặc "unread"
            var data = da.GetGlobalReadersDetails(id, type);
            return Json(new { success = true, list = data }, JsonRequestBehavior.AllowGet);
        }

        // POST: Delete
        [HttpPost]
        public ActionResult DeleteGlobal(int id)
        {
            bool result = da.DeleteGlobalNotification(id);
            return Json(new { success = result, message = result ? "Xóa thành công" : "Lỗi hệ thống" });
        }
        [HttpPost]
        public ActionResult SaveGlobal(int id, string title, string message, string type, string role, string url, DateTime? expiry)
        {
            bool result;
            if (id == 0) // Thêm mới
            {
                result = da.AddGlobalNotification(title, message, type, role, url, expiry);
            }
            else // Cập nhật
            {
                result = da.UpdateGlobalNotification(id, title, message, type, role, url, expiry);
            }
            return Json(new { success = result, message = result ? "Lưu thành công" : "Lỗi hệ thống" });
        }

        [HttpGet]
        public ActionResult GetGlobalDetail(int id)
        {
            var item = da.GetGlobalById(id);
            if (item == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            // Trả về dữ liệu để fill vào form Modal
            return Json(new
            {
                success = true,
                data = new
                {
                    item.Global_Noti_ID,
                    item.Title,
                    item.Message,
                    item.Type,
                    TargetRole = item.Target_Role,
                    TargetUrl = item.Taget_Url,
                    ExpiryDate = item.Expiry_Date?.ToString("yyyy-MM-dd")
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // Action trang chi tiết người xem (Placeholder cho tính năng mở rộng sau)
        public ActionResult Readers(int id, string type)
        {
            // Đây là trang riêng biệt, bạn có thể tạo View Readers.cshtml sau này
            // Hiện tại tạm thời return Content hoặc View chưa tạo
            ViewBag.NotiId = id;
            ViewBag.Type = type;
            return View(); // Cần tạo View Readers.cshtml
        }

        // --- PERSONAL ACTIONS ---

        [HttpPost]
        public ActionResult CreatePersonal(int userId, string title, string message, string type, string url)
        {
            bool result = da.AddPersonalNotification(userId, title, message, type, url);
            return Json(new { success = result, message = result ? "Gửi thông báo thành công" : "User không tồn tại hoặc lỗi hệ thống" });
        }
    }
}