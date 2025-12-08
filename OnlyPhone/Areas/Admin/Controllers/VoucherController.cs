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
    public class VoucherController : Controller
    {
        Xuly xl = new Xuly();
        // GET: Admin/Voucher


        // GET: Admin/Voucher
        public ActionResult Index(string keyword = "", int? status = null, int page = 1)
        {
            var model = xl.GetVouchers(keyword, status, page, 12); // 12 card mỗi trang
            return View(model);
        }

        [HttpPost]
        public ActionResult Save(VoucherViewModel model)
        {
            if (xl.SaveVoucher(model))
                return Json(new { success = true, message = "Lưu thành công!" });
            return Json(new { success = false, message = "Lỗi khi lưu voucher." });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (xl.DeleteVoucher(id))
                return Json(new { success = true });
            return Json(new { success = false, message = "Không thể xóa voucher này (đang sử dụng)." });
        }

        [HttpGet]
        public ActionResult GetDetail(int id)
        {
            var v = xl.GetVoucherDetail(id);
            return Json(v, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Gift(int voucherId, string userIdent)
        {
            var result = xl.GiftVoucherToUser(voucherId, userIdent);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
