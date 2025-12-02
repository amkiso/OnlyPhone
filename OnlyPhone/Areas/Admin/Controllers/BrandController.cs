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
    public class BrandController : Controller
    {

        // GET: Admin/Brand
        Xuly xl = new Xuly();

        public ActionResult manager()
        {
            var model = xl.GetAllBrandsWithSeries();
            ViewBag.Stats = xl.GetBrandStats(); // Giữ nguyên hàm stats cũ của bạn
            return View(model);
        }

        [HttpPost]
        public JsonResult SaveBrand(BrandViewModel model, HttpPostedFileBase logoFile)
        {
            try
            {
                if (logoFile != null)
                {
                    string logoName = xl.UploadBrandLogo(logoFile, Server.MapPath("~"));
                    if (!string.IsNullOrEmpty(logoName)) model.Logo = logoName;
                }

                bool result = xl.SaveBrand(model);
                return Json(new { success = result, message = result ? "Lưu thành công" : "Lỗi khi lưu" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AddSeries(int brandId, string seriesName)
        {
            bool result = xl.AddSeries(brandId, seriesName);
            return Json(new { success = result, message = result ? "Thêm dòng sản phẩm thành công" : "Lỗi: Có thể tên dòng đã tồn tại" });
        }

        [HttpPost]
        public JsonResult DeleteBrand(int id)
        {
            bool result = xl.DeleteBrand(id); // Hàm DeleteBrand cũ của bạn
            return Json(new { success = result, message = result ? "Đã xóa" : "Không thể xóa (Đang có sản phẩm kinh doanh)" });
        }
    }
}