using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlyPhone.Models;

namespace OnlyPhone.Controllers
{
    public class SearchController : Controller
    {
        Xuly xl = new Xuly();

        // GET: Search
        // Trang hiển thị kết quả tìm kiếm đầy đủ
        public ActionResult Index(string q, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy kết quả tìm kiếm
            var searchResult = xl.SearchProducts(q, 70.0M, page, 20);

            ViewBag.Title = $"Kết quả tìm kiếm: {q}";

            return View(searchResult);
        }

        // AJAX: Quick Search - Autocomplete
        [HttpGet]
        public JsonResult QuickSearch(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new { success = false, results = new List<object>() }, JsonRequestBehavior.AllowGet);
                }

                var results = xl.QuickSearchProducts(term, 5);

                var formattedResults = results.Select(r => new
                {
                    id = r.ProductId,
                    name = r.ProductName,
                    price = r.SalePrice,
                    originalPrice = r.OriginalPrice,
                    image = r.ProductImage,
                    similarity = Math.Round(r.SimilarityScore, 0),
                    // URL tạm thời, sẽ update sau khi có trang chi tiết sản phẩm
                    url = Url.Action("ProductDetail", "Products", new { id = r.ProductId })
                }).ToList();

                return Json(new
                {
                    success = true,
                    results = formattedResults,
                    total = formattedResults.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tìm kiếm: " + ex.Message,
                    results = new List<object>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX: Load more results
        [HttpGet]
        public JsonResult LoadMore(string q, int page = 1)
        {
            try
            {
                var searchResult = xl.SearchProducts(q, 30.0M, page, 20);

                var products = searchResult.Products.Select(p => new
                {
                    id = p.product_id,
                    name = p.product_name,
                    price = p.sale_price,
                    originalPrice = p.original_price,
                    image = p.images,
                    supplier = p.supplier_name,
                    isNew = p.is_new,
                    isFeatured = p.is_featured,
                    discountPercent = p.DiscountPercent
                }).ToList();

                return Json(new
                {
                    success = true,
                    products = products,
                    currentPage = searchResult.CurrentPage,
                    totalPages = searchResult.TotalPages,
                    hasMore = searchResult.CurrentPage < searchResult.TotalPages
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}