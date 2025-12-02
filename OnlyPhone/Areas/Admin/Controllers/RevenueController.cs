using On.Areas.Admin;
using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Areas.Admin.Controllers
{
    [AdminSecurity]
    public class RevenueController : Controller
    {
        // GET: Admin/Revenue
        Xuly xl = new Xuly();

        // GET: Admin/Revenue
        public ActionResult Stats(string from, string to)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(from)) fromDate = DateTime.Parse(from);
            if (!string.IsNullOrEmpty(to)) toDate = DateTime.Parse(to);

            var model = xl.GetRevenueStats(fromDate, toDate);

            // Giữ lại giá trị filter để hiển thị lại trên View
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(model);
        }
    }
}