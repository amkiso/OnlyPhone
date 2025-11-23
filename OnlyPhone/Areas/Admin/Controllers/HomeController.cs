using On.Areas.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Areas.Admin.Controllers
{
    [AdminSecurity]
    public class HomeController : Controller
    {
        // GET: Admin/Home
       
        public ActionResult Index()
        {
            return View();
        }
    }
}