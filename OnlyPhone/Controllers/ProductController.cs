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
        Xuly xl = new Xuly();

        // GET: Product/Index - Danh sách sản phẩm với filter theo supplier và series
        public ActionResult Phone(string supplier = null, string series = null, string sort = "featured", int page = 1)
        {
            int pageSize = 15;
            List<Product_Infomation> products;
            int totalProducts;
            string displayTitle = "";

            // Xử lý filter theo supplier và series
            if (!string.IsNullOrEmpty(supplier) && !string.IsNullOrEmpty(series))
            {
                // Lọc theo cả supplier và series
                products = xl.GetProductsBySupplierAndSeries(supplier, series, sort, page, pageSize);
                totalProducts = xl.GetTotalProductsBySupplierAndSeries(supplier, series);
                displayTitle = $"{supplier} - {series}";
            }
            else if (!string.IsNullOrEmpty(supplier))
            {
                // Chỉ lọc theo supplier
                products = xl.GetProductsBySupplier(supplier, sort, page, pageSize);
                totalProducts = xl.GetTotalProductsBySupplier(supplier);
                displayTitle = supplier;
            }
            else if (!string.IsNullOrEmpty(series))
            {
                // Chỉ lọc theo series
                int seriesId = xl.GetSeriesIdByName(series);
                products = xl.GetProductsWithFilter(seriesId, sort, page, pageSize);
                totalProducts = xl.GetTotalProductCount(seriesId);
                displayTitle = series;
            }
            else
            {
                // Không có filter - lấy tất cả
                products = xl.GetProductsWithFilter(0, sort, page, pageSize);
                totalProducts = xl.GetTotalProductCount(0);
                displayTitle = "Tất cả sản phẩm";
            }

            // Áp dụng sorting
            products = ApplySorting(products, sort);

            var model = new ProductFilterViewModel
            {
                Products = products,
                SeriesId = 0,
                CurrentSort = sort,
                CurrentPage = page,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                HasMore = page * pageSize < totalProducts,
                SeriesName = displayTitle,
                CurrentSupplier = supplier,
                CurrentSeries = series
            };

            ViewBag.AllSeries = xl.GetAllSeries();
            ViewBag.AllSuppliers = xl.GetAllSuppliers();

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

            var relatedProducts = xl.GetProductsWithFilter(product.series_id, "featured", 1, 6);

            var model = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts.Where(p => p.product_id != id).Take(4).ToList()
            };

            return View(model);
        }

        // GET: Product/Search - Tìm kiếm sản phẩm
        public ActionResult Search(string q, int series = 0, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Phone");
            }

            int pageSize = 20;
            var result = xl.SearchProducts(q, 30.0M, page, pageSize, series);

            ViewBag.SearchQuery = q;
            ViewBag.AllSeries = xl.GetAllSeries();

            return View(result);
        }

        // GET: Product/BestSeller - Sản phẩm bán chạy
        public ActionResult BestSeller(int page = 1)
        {
            int pageSize = 15;
            var products = xl.GetBestSellerProducts(100); // Lấy 100 sản phẩm bán chạy nhất
            var totalProducts = products.Count;

            // Phân trang
            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ProductFilterViewModel
            {
                Products = pagedProducts,
                SeriesId = 0,
                CurrentSort = "bestseller",
                CurrentPage = page,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                HasMore = page * pageSize < totalProducts,
                SeriesName = "Sản phẩm bán chạy"
            };

            ViewBag.AllSeries = xl.GetAllSeries();
            return View("Phone", model);
        }

        // GET: Product/NewProducts - Sản phẩm mới
        public ActionResult NewProducts(int page = 1)
        {
            int pageSize = 15;
            var products = xl.GetNewProducts(100);
            var totalProducts = products.Count;

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ProductFilterViewModel
            {
                Products = pagedProducts,
                SeriesId = 0,
                CurrentSort = "new",
                CurrentPage = page,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                HasMore = page * pageSize < totalProducts,
                SeriesName = "Sản phẩm mới"
            };

            ViewBag.AllSeries = xl.GetAllSeries();
            return View("Phone", model);
        }

        // GET: Product/Deals - Sản phẩm giảm giá
        public ActionResult Deals(int page = 1)
        {
            int pageSize = 15;
            var products = xl.GetDiscountProducts(100);
            var totalProducts = products.Count;

            var pagedProducts = products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ProductFilterViewModel
            {
                Products = pagedProducts,
                SeriesId = 0,
                CurrentSort = "discount",
                CurrentPage = page,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize),
                HasMore = page * pageSize < totalProducts,
                SeriesName = "DEAL HOT - Giảm giá đặc biệt"
            };

            ViewBag.AllSeries = xl.GetAllSeries();
            return View("Phone", model);
        }

        // POST: Product/AddToCart - Thêm vào giỏ hàng (AJAX)
        [HttpPost]
        public JsonResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
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
                bool result = xl.AddToCart(userId, productId, quantity);

                if (result)
                {
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

        // GET: Product/GetCartCount - Lấy số lượng sản phẩm trong giỏ (AJAX)
        [HttpGet]
        public JsonResult GetCartCount()
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new { success = true, cartCount = 0 }, JsonRequestBehavior.AllowGet);
                }

                int userId = (int)Session["UserID"];
                int cartCount = xl.GetCartItemCount(userId);

                return Json(new { success = true, cartCount = cartCount }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
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

        // Helper method: Apply sorting
        private List<Product_Infomation> ApplySorting(List<Product_Infomation> products, string sortBy)
        {
            switch (sortBy?.ToLower())
            {
                case "new":
                    return products.OrderByDescending(p => p.created_date).ToList();
                case "bestseller":
                    return products.OrderByDescending(p => p.total_sold).ToList();
                case "discount":
                    return products.OrderByDescending(p => p.DiscountPercent).ToList();
                case "price-asc":
                    return products.OrderBy(p => p.sale_price).ToList();
                case "price-desc":
                    return products.OrderByDescending(p => p.sale_price).ToList();
                case "featured":
                default:
                    return products.OrderByDescending(p => p.is_featured)
                                   .ThenByDescending(p => p.created_date)
                                   .ToList();
            }
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