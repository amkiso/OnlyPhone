using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlyPhone.Models;

namespace OnlyPhone.Controllers
{
    public class PromotionController : Controller
    {
        // GET: Promotion
        Xuly xl = new Xuly();
        public ActionResult AllPromotion()
        {
            int? userId = null;
            if (Session["UserID"] != null)
            {
                userId = (int)Session["UserID"];
            }

            var model = xl.GetAllPromotions(userId);
            return View(model);
        }

        // POST: Lưu Voucher
        [HttpPost]
        public JsonResult SaveVoucher(int voucherId)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để lưu voucher.", requireLogin = true });
            }

            int userId = (int)Session["UserID"];
            var result = xl.SaveVoucherToWallet(voucherId, userId);

            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
