using On.Areas.Admin;
using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Areas.Admin.Controllers
{
    public class VoucherController : Controller
    {
        Xuly xl = new Xuly();
        // GET: Admin/Voucher
        public ActionResult Manager()

        {

            return View(xl.GetVouchers().Take(5));
        }
    }
}