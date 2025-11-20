using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlyPhone.Models;

namespace OnlyPhone.Controllers
{
    public class ProductController : Controller
    {
        private Xuly xl = new Xuly();

        // GET: Product/Index - Danh sách tất cả sản phẩm
        public ActionResult Index(int series = 0, string sort = "featured", int page = 1)
        {
            int pageSize = 15;

            // Lấy danh sách sản phẩm với filter
            var products = xl.GetProductsWithFilter(series, sort, page, pageSize);

            // Lấy tổng số sản phẩm
            var totalProducts = xl.GetTotalProductCount(series);

            // Lấy danh sách series cho filter
            var allSeries = xl.GetAllSeries();

            // Tạo view model
            var model = new ProductFilterViewModel
            {
                Products = products,
                SeriesId = series,
                CurrentSort = sort,
                CurrentPage = page,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                HasMore = page * pageSize < totalProducts
            };

            // Lấy tên series nếu có filter
            if (series > 0)
            {
                var seriesInfo = allSeries.FirstOrDefault(s => s.SeriesId == series);
                model.SeriesName = seriesInfo?.SeriesName ?? "";
            }

            // Truyền danh sách series qua ViewBag
            ViewBag.AllSeries = allSeries;

            return View(model);
        }

        // GET: Product/Detail/{id} - Chi tiết sản phẩm
        public ActionResult Detail(int id)
        {
            var product = xl.GetProductDetail(id);

            if (product == null)
            {
                return HttpNotFound("Không tìm thấy sản phẩm");
            }

            // Lấy sản phẩm liên quan (cùng series)
            var relatedProducts = xl.GetProductsWithFilter(product.series_id, "featured", 1, 6);

            // Tạo view model
            var model = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts.Where(p => p.product_id != id).Take(4).ToList()
            };

            return View(model);
        }

        // GET: Product/Search - Tìm kiếm sản phẩm
        public ActionResult Search(string query, int series = 0, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            int pageSize = 20;

            // Sử dụng hàm tìm kiếm
            var result = xl.SearchProducts(query, 30.0M, page, pageSize, series);

            // Truyền query qua ViewBag để hiển thị
            ViewBag.SearchQuery = query;
            ViewBag.AllSeries = xl.GetAllSeries();

            return View(result);
        }

        // GET: Product/BySeries/{seriesId} - Sản phẩm theo series
        public ActionResult BySeries(int id, string sort = "featured", int page = 1)
        {
            return RedirectToAction("Index", new { series = id, sort = sort, page = page });
        }

        // GET: Product/BySupplier/{supplierName} - Sản phẩm theo nhà cung cấp
        public ActionResult BySupplier(string name, int page = 1)
        {
            int pageSize = 15;

            var products = xl.GetProductsBySupplier(name, page, pageSize);
            var totalProducts = xl.GetTotalProductsBySupplier(name);

            var model = new ProductFilterViewModel
            {
                Products = products,
                SeriesId = 0,
                CurrentSort = "featured",
                CurrentPage = page,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                HasMore = page * pageSize < totalProducts,
                SeriesName = name
            };

            return View("Index", model);
        }

        // POST: Product/AddToCart - Thêm vào giỏ hàng (AJAX)
        [HttpPost]
        public JsonResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Kiểm tra user đã đăng nhập chưa
                if (Session["UserID"] == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng",
                        requireLogin = true
                    });
                }

                int userId = (int)Session["UserID"];

                // Thêm vào giỏ hàng
                bool result = xl.AddToCart(userId, productId, quantity);

                if (result)
                {
                    // Lấy số lượng sản phẩm trong giỏ hàng
                    int cartCount = xl.GetCartItemCount(userId);

                    return Json(new
                    {
                        success = true,
                        message = "Đã thêm sản phẩm vào giỏ hàng",
                        cartCount = cartCount
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sản phẩm hết hàng hoặc không tồn tại"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Đã xảy ra lỗi: " + ex.Message
                });
            }
        }

        // POST: Product/QuickSearch - AJAX Quick Search
        [HttpPost]
        public JsonResult QuickSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var results = xl.QuickSearchProducts(term, 5);

            var searchResults = results.Select(r => new
            {
                id = r.ProductId,
                name = r.ProductName,
                price = r.SalePrice.ToString("N0") + " đ",
                originalPrice = r.OriginalPrice > 0 ? r.OriginalPrice.ToString("N0") + " đ" : "",
                image = Url.Content("~/Content/Pic/images/" + r.ProductImage),
                seriesName = r.SeriesName,
                url = Url.Action("Detail", "Product", new { id = r.ProductId })
            });

            return Json(searchResults, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose resources if needed
            }
            base.Dispose(disposing);
        }
    }

    #region ViewModels

    // ViewModel cho trang chi tiết sản phẩm
    public class ProductDetailViewModel
    {
        public Product_Infomation Product { get; set; }
        public List<Product_Infomation> RelatedProducts { get; set; }

        public ProductDetailViewModel()
        {
            RelatedProducts = new List<Product_Infomation>();
        }
    }

    #endregion
}