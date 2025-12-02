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
    public class AccountController : Controller
    {
        // GET: Admin/Account
        Xuly xl = new Xuly();

        // GET: Admin/Users
        public ActionResult Index()
        {
            var users = xl.GetAllUsers();

            // Tính toán số liệu cho Top Stats
            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalOnline = users.Count(u => u.IsOnline && !u.IsLocked);
            ViewBag.TotalLocked = users.Count(u => u.IsLocked);
            ViewBag.TotalCustomers = users.Count(u => u.Role == "Customer");

            return View(users);
        }

        // POST: Cập nhật thông tin
        [HttpPost]
        public JsonResult SaveUser(UserListViewModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Email))
                {
                    return Json(new { success = false, message = "Tên và Email không được để trống" });
                }

                bool result = xl.UpdateUser(model);

                if (result)
                    return Json(new { success = true, message = "Cập nhật thành công!" });
                else
                    return Json(new { success = false, message = "Lỗi khi lưu dữ liệu." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }

        // POST: Xóa tài khoản
        [HttpPost]
        public JsonResult DeleteUser(int id)
        {
            // Check nếu là Admin thì không cho xóa chính mình (demo logic)
            // if (id == currentUserId) return Json(new { success = false, message = "Không thể xóa chính mình" });

            bool result = xl.DeleteUser(id);
            if (result)
                return Json(new { success = true, message = "Đã xóa tài khoản." });
            else
                return Json(new { success = false, message = "Không thể xóa (Tài khoản đã có dữ liệu đơn hàng). Hãy thử Khóa tài khoản." });
        }
    }
}