using Newtonsoft.Json;
using On.Areas.Admin;
using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Areas.Admin.Controllers
{
    
    public class ProductController : Controller
    {
        Xuly xl = new Xuly();

        // GET: Admin/Product/Manager
        public ActionResult Manager(string status = "all", string keyword = "", int page = 1, int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

            // 1. Lấy dữ liệu phân trang + filter
            var pagedResult = xl.GetAdminProducts(status, keyword, page, pageSize, "new");
            var totalPages = (int)Math.Ceiling((double)pagedResult.TotalItems / pageSize);

            // Nếu page vượt quá tổng trang (sau khi filter) thì lùi về trang cuối
            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
                pagedResult = xl.GetAdminProducts(status, keyword, page, pageSize, "new");
            }

            // 2. Tính toán số liệu Top Cards
            ViewBag.TotalSelling = pagedResult.TotalSelling;
            ViewBag.TotalLowStock = pagedResult.TotalLowStock;
            ViewBag.TotalOutStock = pagedResult.TotalOutStock;
            ViewBag.TotalSold = pagedResult.TotalSold;

            // 3. Lấy danh sách cho Dropdown
            ViewBag.SeriesList = xl.GetAllSeries();
            ViewBag.SupplierList = xl.GetAllSuppliers();

            // 4. Thông tin phân trang
            ViewBag.TotalItems = pagedResult.TotalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.StatusFilter = status;
            ViewBag.Keyword = keyword;
            ViewBag.FilteredProducts = JsonConvert.SerializeObject(pagedResult.Items);

            return View(pagedResult.Items);
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