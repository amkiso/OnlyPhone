using On.Areas.Admin;
using OnlyPhone.Areas.Admin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlyPhone.Models;

namespace OnlyPhone.Areas.Admin.Controllers
{
    [AdminSecurity]
    public class HomeController : Controller
    {
        // GET: Admin/Home

        Xuly xl = new Xuly();
        public ActionResult Dashbroad()
        {
            var model = xl.GetDashboardData();
            return View(model);
        }

        // API: Lấy dữ liệu mới (gọi bằng AJAX)
        [HttpGet]
        public JsonResult GetStats()
        {
            var data = xl.GetDashboardData();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        
    }
}