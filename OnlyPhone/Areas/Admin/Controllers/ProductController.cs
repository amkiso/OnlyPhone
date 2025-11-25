using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlyPhone.Models;

namespace OnlyPhone.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        Xuly xl = new Xuly();

        // GET: Admin/Product/Manager
        public ActionResult Manager()
        {
            // 1. Lấy dữ liệu (PageSize lớn để lấy hết danh sách quản lý)
            var products = xl.GetProductsWithFilter(0, "new", 1, 2000);

            // 2. Tính toán số liệu Top Cards
            ViewBag.TotalSelling = products.Count(p => p.product_status == "selling" && p.current_Quantity > 0);
            ViewBag.TotalLowStock = products.Count(p => p.current_Quantity > 0 && p.current_Quantity <= 5);
            ViewBag.TotalOutStock = products.Count(p => p.product_status == "out of stock" || p.current_Quantity == 0);
            ViewBag.TotalSold = products.Sum(p => p.total_sold);

            // 3. Lấy danh sách cho Dropdown
            ViewBag.SeriesList = xl.GetAllSeries();
            ViewBag.SupplierList = xl.GetAllSuppliers();

            return View(products);
        }

        // POST: Save Product
        [HttpPost]
        public JsonResult SaveProduct(Product_Infomation model, HttpPostedFileBase imageFile)
        {
            try
            {
                // Upload ảnh
                if (imageFile != null)
                {
                    string uploaded = xl.UploadProductImage(imageFile, Server.MapPath("~"));
                    if (!string.IsNullOrEmpty(uploaded)) model.images = uploaded;
                }

                // Xử lý Description từ textarea
                string rawDesc = Request.Form["raw_description"];
                if (!string.IsNullOrEmpty(rawDesc))
                {
                    model.product_description = rawDesc.Split('\n').ToList();
                }

                bool result = false;
                // ID <= 0 => Thêm mới, > 0 => Cập nhật
                if (model.product_id <= 0) result = xl.AddProduct(model);
                else result = xl.UpdateProduct(model);

                if (result) return Json(new { success = true, message = "Thao tác thành công!" });
                else return Json(new { success = false, message = "Lỗi khi lưu vào CSDL." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}