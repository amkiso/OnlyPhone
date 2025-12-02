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
    }
}