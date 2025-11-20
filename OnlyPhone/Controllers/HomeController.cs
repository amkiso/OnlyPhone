using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Controllers
{
    public class HomeController : Controller
    {
        SQLDataClassesDataContext db = new SQLDataClassesDataContext();
     
        Xuly xl = new Xuly();

        // GET: Home/Index

        public ActionResult Index()
        {
            var model = xl.GetHomePageData();

            return View(model);
        }

        public ActionResult Products(int series = 0, string sort = "featured", int page = 1)
        {
            int pageSize = 15;

            // Lấy danh sách sản phẩm với filter
            var products = xl.GetProductsWithFilter(series, sort, page, pageSize);

            // Lấy tổng số sản phẩm
            var totalProducts = xl.GetTotalProductCount(series);

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
                var seriesInfo = xl.GetAllSeries().FirstOrDefault(s => s.SeriesId == series);
                model.SeriesName = seriesInfo?.SeriesName ?? "";
            }

            return View(model);
        }

        // GET: Home/ProductDetail - Chi tiết sản phẩm
        public ActionResult ProductDetail(int id)
        {
            // TODO: Implement chi tiết sản phẩm
            // Bạn có thể tạo method GetProductDetail(int id) trong Xuly.cs

            return View();
        }

        // GET: Home/Search - Tìm kiếm sản phẩm
        public ActionResult Search(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Products");
            }

            int pageSize = 20;

            // Sử dụng hàm tìm kiếm đã có trong Xuly.cs
            var result = xl.SearchProducts(query, 30.0M, page, pageSize);

            return View(result);
        }

        // POST: Home/QuickSearch - AJAX Quick Search
        [HttpPost]
        public JsonResult QuickSearch(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            // Sử dụng hàm QuickSearch đã có
            var results = xl.QuickSearchProducts(term, 5);

            // Format kết quả cho autocomplete
            var searchResults = results.Select(r => new
            {
                id = r.ProductId,
                name = r.ProductName,
                price = r.SalePrice.ToString("N0") + " đ",
                originalPrice = r.OriginalPrice > 0 ? r.OriginalPrice.ToString("N0") + " đ" : "",
                image = Url.Content("~/Content/Pic/images/" + r.ProductImage),
                seriesName = r.SeriesName,
                url = Url.Action("ProductDetail", "Home", new { id = r.ProductId })
            });

            return Json(searchResults, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
       
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [ChildActionOnly]
        public ActionResult HeaderParaticalView()
        {
            var model = new UserDataViewModel();

            // Kiểm tra người dùng đã đăng nhập chưa
            if (Session["UserID"] != null)
            {
                model.IsLoggedIn = true;
                int userId = Convert.ToInt32(Session["UserID"]);
                model.UserRole = Session["UserRole"]?.ToString()?.ToLower() ?? "customer";

                // Lấy số lượng thông báo chưa đọc
                model.NotificationCount = xl.GetUnreadNotificationCount(userId);

                // Lấy danh sách thông báo (10 thông báo gần nhất)
                model.Notifications = xl.GetUserNotifications(userId, 10);

                // Lấy số lượng sản phẩm trong giỏ hàng
                model.CartCount = xl.GetCartItemCount(userId);

                // Lấy danh sách sản phẩm trong giỏ hàng
                model.CartItems = xl.GetCartItems(userId);
            }
            else
            {
                model.IsLoggedIn = false;
                model.CartCount = 0;
                model.NotificationCount = 0;
            }

            return PartialView("~/Views/Shared/HeaderParaticalView.cshtml", model);
        }
        // Thêm method này vào HomeController.cs của bạn

        [HttpGet]
        public JsonResult GetCartCount()
        {
            try
            {
                int count = 0;

                if (Session["UserID"] != null)
                {
                    int userId = Convert.ToInt32(Session["UserID"]);
                    count = xl.GetCartItemCount(userId);
                }

                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { count = 0, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Kiểm tra user đã đăng nhập chưa
                if (Session["UserID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng" });
                }

                int userId = (int)Session["UserID"];

                // Sử dụng hàm AddToCart đã có
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
                    return Json(new { success = false, message = "Không thể thêm sản phẩm vào giỏ hàng" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}

        

    
