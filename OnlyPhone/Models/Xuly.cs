using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OnlyPhone.Models;

namespace OnlyPhone.Models
{
    public class Xuly
    {
        SQLDataClassesDataContext da = new SQLDataClassesDataContext();

        public Xuly() { }

        // =====================================================
        // PRODUCT METHODS
        // =====================================================

        // Method lấy tất cả sản phẩm (compatibility với code cũ)
        public List<Product_Infomation> itemproduct()
        {
            // Sử dụng method mới với tham số mặc định (0 = all series)
            return GetProductsWithFilter(0, "featured", 1, 1000);
        }

        // Method để lấy sản phẩm với filter và sort (đã thay Type_Product_ID thành Series_ID)
        public List<Product_Infomation> GetProductsWithFilter(
            int seriesID,
            string sortBy = "featured",
            int pageNumber = 1,
            int pageSize = 15)
        {
            try
            {
                // Gọi stored procedure với tham số SeriesID thay vì TypeProductID
                var result = da.sp_GetProductsWithFilter(
                    seriesID == 0 ? (int?)null : seriesID,  // 0 = lấy tất cả
                    sortBy,
                    pageNumber,
                    pageSize
                ).ToList();

                List<Product_Infomation> products = new List<Product_Infomation>();

                foreach (var item in result)
                {
                    var product = new Product_Infomation
                    {
                        product_id = item.Product_ID,
                        product_name = item.Product_name,
                        sale_price = item.Sale_Price ?? 0,
                        original_price = item.Original_Price ?? 0,
                        current_Quantity = item.Current_Quantity,
                        product_status = item.Product_Status,
                        images = item.Product_Image,
                        series_name = item.SeriesName,  // Thay type_product bằng series_name
                        series_id = item.Series_id,     // Thêm series_id
                        supplier_name = item.supplier_name,
                        is_featured = item.Is_Featured ?? false,
                        is_new = item.Is_New ?? false,
                        total_sold = item.Total_Sold,
                        created_date = item.Created_Date ?? DateTime.Now,
                        product_description = ParseProductDescription(item.Descriptions)
                    };

                    products.Add(product);
                }

                return products;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductsWithFilter: {ex.Message}");
                return new List<Product_Infomation>();
            }
        }

        // Method đếm tổng số sản phẩm (cho pagination) - đã thay Type_Product_ID thành Series_id
        public int GetTotalProductCount(int seriesID)
        {
            try
            {
                var query = da.Products.Where(p => p.Product_Status == "selling");

                if (seriesID > 0)
                {
                    query = query.Where(p => p.Series_id == seriesID);
                }

                return query.Count();
            }
            catch
            {
                return 0;
            }
        }

        // Method lấy sản phẩm theo series (thay vì theo loại)
        public List<Product_Infomation> GetProductsBySeries(int seriesID, string sortBy = "featured")
        {
            return GetProductsWithFilter(seriesID, sortBy, 1, 1000);
        }

        // Method mới: Lấy danh sách tất cả series
        public List<SeriesInfo> GetAllSeries()
        {
            try
            {
                return da.PhoneSeries
                    .Select(s => new SeriesInfo
                    {
                        SeriesId = s.Series_id,
                        SeriesName = s.SeriesName,
                        ProductCount = da.Products
                            .Where(p => p.Series_id == s.Series_id && p.Product_Status == "selling")
                            .Count()
                    })
                    .OrderBy(s => s.SeriesName)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAllSeries: {ex.Message}");
                return new List<SeriesInfo>();
            }
        }

        // Method parse description từ chuỗi hoặc JSON
        private List<string> ParseProductDescription(string description)
        {
            try
            {
                if (string.IsNullOrEmpty(description))
                    return new List<string> { "Đang cập nhật thông tin..." };

                // Nếu có dấu ; thì split theo ;
                if (description.Contains(";"))
                {
                    return description.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim())
                                     .ToList();
                }

                // Nếu có .json thì đọc file JSON
                if (description.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    string fullPath = HttpContext.Current.Server.MapPath($"~/Content/Json/{description}");

                    if (System.IO.File.Exists(fullPath))
                    {
                        string jsonContent = System.IO.File.ReadAllText(fullPath);
                        var descriptionObj = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(jsonContent);

                        if (descriptionObj != null && descriptionObj.ContainsKey("description"))
                        {
                            return descriptionObj["description"];
                        }
                    }
                }

                // Nếu không phải cả 2 trường hợp trên, trả về list có 1 phần tử
                return new List<string> { description };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing description: {ex.Message}");
                return new List<string> { "Lỗi tải thông tin sản phẩm" };
            }
        }

        // Method update Is_Featured cho sản phẩm (dành cho Admin)
        public bool UpdateProductFeaturedStatus(int productId, bool isFeatured)
        {
            try
            {
                var product = da.Products.FirstOrDefault(p => p.Product_ID == productId);
                if (product != null)
                {
                    product.Is_Featured = isFeatured;
                    da.SubmitChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // Method update Is_New cho sản phẩm (dành cho Admin)
        public bool UpdateProductNewStatus(int productId, bool isNew)
        {
            try
            {
                var product = da.Products.FirstOrDefault(p => p.Product_ID == productId);
                if (product != null)
                {
                    product.Is_New = isNew;
                    da.SubmitChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // =====================================================
        // CART METHODS
        // =====================================================

        // Method thêm sản phẩm vào giỏ hàng
        public bool AddToCart(int userId, int productId, int quantity = 1)
        {
            try
            {
                // Kiểm tra sản phẩm tồn tại và còn hàng
                var product = da.Products.FirstOrDefault(p => p.Product_ID == productId);
                if (product == null || product.Current_Quantity < quantity)
                {
                    return false;
                }

                // Lấy hoặc tạo giỏ hàng cho user
                var cart = da.shopping_carts.FirstOrDefault(c => c.ID_user == userId);
                if (cart == null)
                {
                    cart = new shopping_cart
                    {
                        ID_user = userId,
                        day_create = DateTime.Now,
                        update_at = DateTime.Now
                    };
                    da.shopping_carts.InsertOnSubmit(cart);
                    da.SubmitChanges();
                }

                // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
                var existingItem = da.cart_items.FirstOrDefault(
                    ci => ci.cart_ID == cart.cart_ID && ci.Product_ID == productId
                );

                if (existingItem != null)
                {
                    // Cập nhật số lượng
                    existingItem.quantity += quantity;
                    cart.update_at = DateTime.Now;
                }
                else
                {
                    // Thêm mới
                    var cartItem = new cart_item
                    {
                        cart_ID = cart.cart_ID,
                        Product_ID = productId,
                        quantity = quantity,
                        Sale_Price = product.Sale_Price
                    };
                    da.cart_items.InsertOnSubmit(cartItem);
                    cart.update_at = DateTime.Now;
                }

                da.SubmitChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart: {ex.Message}");
                return false;
            }
        }

        // Get or Create Cart
        public shopping_cart GetOrCreateCart(int userId)
        {
            var cart = da.shopping_carts.FirstOrDefault(c => c.ID_user == userId);

            if (cart == null)
            {
                cart = new shopping_cart
                {
                    ID_user = userId,
                    day_create = DateTime.Now,
                    update_at = DateTime.Now
                };
                da.shopping_carts.InsertOnSubmit(cart);
                da.SubmitChanges();
            }

            return cart;
        }

        // Lấy số lượng sản phẩm trong giỏ hàng
        public int GetCartItemCount(int userId)
        {
            try
            {
                var cart = da.shopping_carts.FirstOrDefault(c => c.ID_user == userId);

                if (cart == null)
                    return 0;

                return da.cart_items
                    .Where(ci => ci.cart_ID == cart.cart_ID)
                    .Count();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetCartItemCount: " + ex.Message);
                return 0;
            }
        }

        // Lấy danh sách sản phẩm trong giỏ hàng
        public List<CartItem> GetCartItems(int userId)
        {
            try
            {
                var cart = da.shopping_carts.FirstOrDefault(c => c.ID_user == userId);

                if (cart == null)
                    return new List<CartItem>();

                List<CartItem> cartItems = (from ci in da.cart_items
                                            where ci.cart_ID == cart.cart_ID
                                            join p in da.Products on ci.Product_ID equals p.Product_ID
                                            select new CartItem
                                            {
                                                CartItemId = ci.cart_item_ID,
                                                ProductId = p.Product_ID,
                                                ProductName = p.Product_name,
                                                ImageUrl = p.Product_Image,
                                                Price = ci.Sale_Price ?? p.Sale_Price ?? 0,
                                                Quantity = ci.quantity ?? 1
                                            }).ToList();

                return cartItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetCartItems: " + ex.Message);
                return new List<CartItem>();
            }
        }

        // =====================================================
        // NOTIFICATION METHODS
        // =====================================================

        // Lấy số lượng thông báo chưa đọc
        public int GetUnreadNotificationCount(int userId)
        {
            try
            {
                return da.Notifications
                    .Where(n => n.ID_user == userId && n.IsRead == false)
                    .Count();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetUnreadNotificationCount: " + ex.Message);
                return 0;
            }
        }

        // Lấy danh sách thông báo
        public List<NotificationItem> GetUserNotifications(int userId, int count)
        {
            try
            {
                return da.Notifications
                    .Where(n => n.ID_user == userId)
                    .OrderByDescending(n => n.Created_At)
                    .Take(count)
                    .Select(n => new NotificationItem
                    {
                        Id = n.Notification_ID,
                        Title = n.Title,
                        Content = n.Message,
                        IsRead = n.IsRead ?? false,
                        CreatedDate = n.Created_At ?? DateTime.Now,
                        Type = n.Type,
                        RelatedId = n.Related_ID
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetUserNotifications: " + ex.Message);
                return new List<NotificationItem>();
            }
        }
        public List<slide> GetSlideList()
        {
            return da.slides.Where(t=>t.status == true).ToList();
        }
        // =====================================================
        // SECURITY METHODS
        // =====================================================

        // Hash password
        public string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Verify password
        public bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return HashPassword(inputPassword) == storedPassword;
        }

        // =====================================================
        // SEARCH METHODS
        // =====================================================

        // Quick search cho autocomplete (hiển thị dropdown khi gõ)
        public List<SearchResultItem> QuickSearchProducts(string searchTerm, int maxResults = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
                    return new List<SearchResultItem>();

                var results = da.sp_QuickSearchProducts(searchTerm, maxResults).ToList();

                return results.Select(r => new SearchResultItem
                {
                    ProductId = r.Product_ID,
                    ProductName = r.Product_name,
                    SalePrice = r.Sale_Price ?? 0,
                    OriginalPrice = r.Original_Price ?? 0,
                    ProductImage = r.Product_Image,
                    SeriesName = r.SeriesName,  // Thay Type_Product_name bằng SeriesName
                    SimilarityScore = r.SimilarityScore ?? 0
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in QuickSearchProducts: {ex.Message}");
                return new List<SearchResultItem>();
            }
        }

        // Full search cho trang kết quả tìm kiếm (đã thay Type_Product_ID thành Series_ID)
        public SearchResultViewModel SearchProducts(
            string searchTerm,
            decimal minSimilarity = 30.0M,
            int pageNumber = 1,
            int pageSize = 20,
            int seriesID = 0)  // Thay typeProductID thành seriesID
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return new SearchResultViewModel
                    {
                        Products = new List<Product_Infomation>(),
                        SearchTerm = searchTerm,
                        TotalResults = 0,
                        CurrentPage = pageNumber,
                        PageSize = pageSize,
                        TotalPages = 0
                    };
                }

                var results = da.sp_SearchProducts(
                    searchTerm,
                    minSimilarity,
                    pageNumber,
                    pageSize,
                    seriesID == 0 ? (int?)null : seriesID  // 0 = tìm kiếm tất cả series
                ).ToList();

                var products = results.Select(r => new Product_Infomation
                {
                    product_id = r.Product_ID,
                    product_name = r.Product_name,
                    sale_price = r.Sale_Price ?? 0,
                    original_price = r.Original_Price ?? 0,
                    images = r.Product_Image,
                    current_Quantity = r.Current_Quantity,
                    product_status = r.Product_Status,
                    series_name = r.SeriesName,  // Thay type_product bằng series_name
                    supplier_name = r.supplier_name,
                    is_featured = r.Is_Featured ?? false,
                    is_new = r.Is_New ?? false,
                    total_sold = r.Total_Sold,
                    product_description = new List<string>() // Có thể load sau nếu cần
                }).ToList();

                int totalResults = results.FirstOrDefault()?.TotalRecords ?? 0;

                return new SearchResultViewModel
                {
                    Products = products,
                    SearchTerm = searchTerm,
                    TotalResults = totalResults,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalResults / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SearchProducts: {ex.Message}");
                return new SearchResultViewModel
                {
                    Products = new List<Product_Infomation>(),
                    SearchTerm = searchTerm,
                    TotalResults = 0,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = 0
                };
            }
        }
        /// <summary>
        /// Hàm chính lấy tất cả dữ liệu cho trang Index
        /// </summary>
        public IndexpageData GetHomePageData()
        {
            try
            {
                var model = new IndexpageData
                {
                    // Lấy slides
                    SlideImages = GetSlideList(),

                    // Lấy các sản phẩm theo danh mục
                    FeaturedProducts = GetFeaturedProducts(8),
                    NewProducts = GetNewProducts(8),
                    BestSellerProducts = GetBestSellerProducts(8),
                    DiscountProducts = GetDiscountProducts(8),

                    // Lấy sản phẩm theo thương hiệu
                    AppleProducts = GetProductsByBrand("Apple", 6),
                    SamsungProducts = GetProductsByBrand("Samsung", 6),
                    XiaomiProducts = GetProductsByBrand("Xiaomi", 6),
                    OppoProducts = GetProductsByBrand("OPPO", 6),
                    VivoProducts = GetProductsByBrand("Vivo", 6),
                    RealmeProducts = GetProductsByBrand("Realme", 6),

                    // Lấy danh sách series
                    AllSeries = GetAllSeries()
                };

                return model;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetHomePageData: {ex.Message}");
                return new IndexpageData(); // Trả về model rỗng nếu có lỗi
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm nổi bật (Is_Featured = true)
        /// </summary>
        private List<Product_Infomation> GetFeaturedProducts(int count)
        {
            try
            {
                var products = (from p in da.Products
                                join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                                where p.Is_Featured == true && p.Product_Status == "selling"
                                orderby p.Created_Date descending
                                select new
                                {
                                    Product = p,
                                    SupplierName = s.supplier_name,
                                    SeriesName = da.PhoneSeries
                                        .Where(ps => ps.Series_id == p.Series_id)
                                        .Select(ps => ps.SeriesName)
                                        .FirstOrDefault(),
                                    TotalSold = da.Orders_items
                                        .Where(oi => oi.Product_ID == p.Product_ID)
                                        .Sum(oi => (int?)oi.quantity) ?? 0
                                })
                               .Take(count)
                               .ToList();

                return products.Select(x => new Product_Infomation
                {
                    product_id = x.Product.Product_ID,
                    product_name = x.Product.Product_name,
                    sale_price = x.Product.Sale_Price ?? 0,
                    original_price = x.Product.Original_Price ?? 0,
                    current_Quantity = x.Product.Current_Quantity,
                    product_status = x.Product.Product_Status,
                    images = x.Product.Product_Image,
                    series_name = x.SeriesName,
                    series_id = x.Product.Series_id,
                    supplier_name = x.SupplierName,
                    is_featured = x.Product.Is_Featured ?? false,
                    is_new = x.Product.Is_New ?? false,
                    total_sold = x.TotalSold,
                    created_date = x.Product.Created_Date ?? DateTime.Now,
                    product_description = ParseProductDescription(x.Product.Descriptions)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetFeaturedProducts: {ex.Message}");
                return new List<Product_Infomation>();
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm mới (Is_New = true)
        /// </summary>
        private List<Product_Infomation> GetNewProducts(int count)
        {
            try
            {
                var products = (from p in da.Products
                                join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                                where p.Is_New == true && p.Product_Status == "selling"
                                orderby p.Created_Date descending
                                select new
                                {
                                    Product = p,
                                    SupplierName = s.supplier_name,
                                    SeriesName = da.PhoneSeries
                                        .Where(ps => ps.Series_id == p.Series_id)
                                        .Select(ps => ps.SeriesName)
                                        .FirstOrDefault(),
                                    TotalSold = da.Orders_items
                                        .Where(oi => oi.Product_ID == p.Product_ID)
                                        .Sum(oi => (int?)oi.quantity) ?? 0
                                })
                               .Take(count)
                               .ToList();

                return products.Select(x => new Product_Infomation
                {
                    product_id = x.Product.Product_ID,
                    product_name = x.Product.Product_name,
                    sale_price = x.Product.Sale_Price ?? 0,
                    original_price = x.Product.Original_Price ?? 0,
                    current_Quantity = x.Product.Current_Quantity,
                    product_status = x.Product.Product_Status,
                    images = x.Product.Product_Image,
                    series_name = x.SeriesName,
                    series_id = x.Product.Series_id,
                    supplier_name = x.SupplierName,
                    is_featured = x.Product.Is_Featured ?? false,
                    is_new = x.Product.Is_New ?? false,
                    total_sold = x.TotalSold,
                    created_date = x.Product.Created_Date ?? DateTime.Now,
                    product_description = ParseProductDescription(x.Product.Descriptions)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetNewProducts: {ex.Message}");
                return new List<Product_Infomation>();
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm bán chạy nhất (theo tổng số lượng đã bán)
        /// </summary>
        private List<Product_Infomation> GetBestSellerProducts(int count)
        {
            try
            {
                var productSales = (from oi in da.Orders_items
                                    group oi by oi.Product_ID into g
                                    select new
                                    {
                                        ProductId = g.Key,
                                        TotalSold = g.Sum(x => x.quantity ?? 0)
                                    })
                                   .OrderByDescending(x => x.TotalSold)
                                   .Take(count)
                                   .ToList();

                var productIds = productSales.Select(ps => ps.ProductId).ToList();

                var products = (from p in da.Products
                                join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                                where productIds.Contains(p.Product_ID) && p.Product_Status == "selling"
                                select new
                                {
                                    Product = p,
                                    SupplierName = s.supplier_name,
                                    SeriesName = da.PhoneSeries
                                        .Where(ps => ps.Series_id == p.Series_id)
                                        .Select(ps => ps.SeriesName)
                                        .FirstOrDefault(),
                                    TotalSold = productSales
                                        .Where(ps => ps.ProductId == p.Product_ID)
                                        .Select(ps => ps.TotalSold)
                                        .FirstOrDefault()
                                })
                               .ToList();

                return products
                    .OrderByDescending(x => x.TotalSold)
                    .Select(x => new Product_Infomation
                    {
                        product_id = x.Product.Product_ID,
                        product_name = x.Product.Product_name,
                        sale_price = x.Product.Sale_Price ?? 0,
                        original_price = x.Product.Original_Price ?? 0,
                        current_Quantity = x.Product.Current_Quantity,
                        product_status = x.Product.Product_Status,
                        images = x.Product.Product_Image,
                        series_name = x.SeriesName,
                        series_id = x.Product.Series_id,
                        supplier_name = x.SupplierName,
                        is_featured = x.Product.Is_Featured ?? false,
                        is_new = x.Product.Is_New ?? false,
                        total_sold = x.TotalSold,
                        created_date = x.Product.Created_Date ?? DateTime.Now,
                        product_description = ParseProductDescription(x.Product.Descriptions)
                    }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetBestSellerProducts: {ex.Message}");
                return new List<Product_Infomation>();
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm giảm giá (có Original_Price > Sale_Price)
        /// </summary>
        private List<Product_Infomation> GetDiscountProducts(int count)
        {
            try
            {
                var products = (from p in da.Products
                                join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                                where p.Product_Status == "selling"
                                      && p.Original_Price.HasValue
                                      && p.Sale_Price < p.Original_Price
                                select new
                                {
                                    Product = p,
                                    SupplierName = s.supplier_name,
                                    SeriesName = da.PhoneSeries
                                        .Where(ps => ps.Series_id == p.Series_id)
                                        .Select(ps => ps.SeriesName)
                                        .FirstOrDefault(),
                                    TotalSold = da.Orders_items
                                        .Where(oi => oi.Product_ID == p.Product_ID)
                                        .Sum(oi => (int?)oi.quantity) ?? 0,
                                    DiscountPercent = p.Original_Price.Value > 0
                                        ? ((p.Original_Price.Value - (p.Sale_Price ?? 0)) / p.Original_Price.Value) * 100
                                        : 0
                                })
                               .OrderByDescending(x => x.DiscountPercent)
                               .Take(count)
                               .ToList();

                return products.Select(x => new Product_Infomation
                {
                    product_id = x.Product.Product_ID,
                    product_name = x.Product.Product_name,
                    sale_price = x.Product.Sale_Price ?? 0,
                    original_price = x.Product.Original_Price ?? 0,
                    current_Quantity = x.Product.Current_Quantity,
                    product_status = x.Product.Product_Status,
                    images = x.Product.Product_Image,
                    series_name = x.SeriesName,
                    series_id = x.Product.Series_id,
                    supplier_name = x.SupplierName,
                    is_featured = x.Product.Is_Featured ?? false,
                    is_new = x.Product.Is_New ?? false,
                    total_sold = x.TotalSold,
                    created_date = x.Product.Created_Date ?? DateTime.Now,
                    product_description = ParseProductDescription(x.Product.Descriptions)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDiscountProducts: {ex.Message}");
                return new List<Product_Infomation>();
            }
        }

        /// <summary>
        /// Lấy sản phẩm theo thương hiệu (supplier name)
        /// </summary>
        private BrandProducts GetProductsByBrand(string brandName, int count)
        {
            try
            {
                var products = (from p in da.Products
                                join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                                where s.supplier_name == brandName && p.Product_Status == "selling"
                                orderby p.Is_Featured descending, p.Created_Date descending
                                select new
                                {
                                    Product = p,
                                    SupplierName = s.supplier_name,
                                    SeriesName = da.PhoneSeries
                                        .Where(ps => ps.Series_id == p.Series_id)
                                        .Select(ps => ps.SeriesName)
                                        .FirstOrDefault(),
                                    TotalSold = da.Orders_items
                                        .Where(oi => oi.Product_ID == p.Product_ID)
                                        .Sum(oi => (int?)oi.quantity) ?? 0
                                })
                               .Take(count)
                               .ToList();

                // Lấy tổng số sản phẩm của brand
                var totalCount = da.Products
                    .Join(da.suppliers, p => p.supplier_ID, s => s.supplier_ID, (p, s) => new { p, s })
                    .Where(x => x.s.supplier_name == brandName && x.p.Product_Status == "selling")
                    .Count();

                // Lấy SeriesId chính của brand (series có nhiều sản phẩm nhất)
                var mainSeriesId = products
                    .GroupBy(x => x.Product.Series_id)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault();

                return new BrandProducts
                {
                    BrandName = brandName,
                    SeriesId = mainSeriesId,
                    TotalProducts = totalCount,
                    Products = products.Select(x => new Product_Infomation
                    {
                        product_id = x.Product.Product_ID,
                        product_name = x.Product.Product_name,
                        sale_price = x.Product.Sale_Price ?? 0,
                        original_price = x.Product.Original_Price ?? 0,
                        current_Quantity = x.Product.Current_Quantity,
                        product_status = x.Product.Product_Status,
                        images = x.Product.Product_Image,
                        series_name = x.SeriesName,
                        series_id = x.Product.Series_id,
                        supplier_name = x.SupplierName,
                        is_featured = x.Product.Is_Featured ?? false,
                        is_new = x.Product.Is_New ?? false,
                        total_sold = x.TotalSold,
                        created_date = x.Product.Created_Date ?? DateTime.Now,
                        product_description = ParseProductDescription(x.Product.Descriptions)
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductsByBrand: {ex.Message}");
                return new BrandProducts
                {
                    BrandName = brandName,
                    SeriesId = 0,
                    TotalProducts = 0,
                    Products = new List<Product_Infomation>()
                };
            }
        }
        /// <summary>
        /// Lấy thông tin chi tiết sản phẩm theo ID
        /// </summary>
        public Product_Infomation GetProductDetail(int productId)
        {
            try
            {
                var product = (from p in da.Products
                               join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                               where p.Product_ID == productId
                               select new
                               {
                                   Product = p,
                                   SupplierName = s.supplier_name,
                                   SeriesName = da.PhoneSeries
                                       .Where(ps => ps.Series_id == p.Series_id)
                                       .Select(ps => ps.SeriesName)
                                       .FirstOrDefault(),
                                   TotalSold = da.Orders_items
                                       .Where(oi => oi.Product_ID == p.Product_ID)
                                       .Sum(oi => (int?)oi.quantity) ?? 0
                               })
                              .FirstOrDefault();

                if (product == null)
                    return null;

                return new Product_Infomation
                {
                    product_id = product.Product.Product_ID,
                    product_name = product.Product.Product_name,
                    sale_price = product.Product.Sale_Price ?? 0,
                    original_price = product.Product.Original_Price ?? 0,
                    current_Quantity = product.Product.Current_Quantity,
                    product_status = product.Product.Product_Status,
                    images = product.Product.Product_Image,
                    series_name = product.SeriesName,
                    series_id = product.Product.Series_id,
                    supplier_name = product.SupplierName,
                    is_featured = product.Product.Is_Featured ?? false,
                    is_new = product.Product.Is_New ?? false,
                    total_sold = product.TotalSold,
                    created_date = product.Product.Created_Date ?? DateTime.Now,
                    product_description = ParseProductDescription(product.Product.Descriptions)
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductDetail: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Lấy sản phẩm theo supplier (nhà cung cấp)
        /// </summary>
        public List<Product_Infomation> GetProductsBySupplier(string supplierName, int page = 1, int pageSize = 15)
        {
            try
            {
                var products = (from p in da.Products
                                join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                                where s.supplier_name == supplierName && p.Product_Status == "selling"
                                orderby p.Is_Featured descending, p.Created_Date descending
                                select new
                                {
                                    Product = p,
                                    SupplierName = s.supplier_name,
                                    SeriesName = da.PhoneSeries
                                        .Where(ps => ps.Series_id == p.Series_id)
                                        .Select(ps => ps.SeriesName)
                                        .FirstOrDefault(),
                                    TotalSold = da.Orders_items
                                        .Where(oi => oi.Product_ID == p.Product_ID)
                                        .Sum(oi => (int?)oi.quantity) ?? 0
                                })
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToList();

                return products.Select(x => new Product_Infomation
                {
                    product_id = x.Product.Product_ID,
                    product_name = x.Product.Product_name,
                    sale_price = x.Product.Sale_Price ?? 0,
                    original_price = x.Product.Original_Price ?? 0,
                    current_Quantity = x.Product.Current_Quantity,
                    product_status = x.Product.Product_Status,
                    images = x.Product.Product_Image,
                    series_name = x.SeriesName,
                    series_id = x.Product.Series_id,
                    supplier_name = x.SupplierName,
                    is_featured = x.Product.Is_Featured ?? false,
                    is_new = x.Product.Is_New ?? false,
                    total_sold = x.TotalSold,
                    created_date = x.Product.Created_Date ?? DateTime.Now,
                    product_description = ParseProductDescription(x.Product.Descriptions)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetProductsBySupplier: {ex.Message}");
                return new List<Product_Infomation>();
            }
        }

        /// <summary>
        /// Đếm tổng số sản phẩm theo supplier
        /// </summary>
        public int GetTotalProductsBySupplier(string supplierName)
        {
            try
            {
                return da.Products
                    .Join(da.suppliers, p => p.supplier_ID, s => s.supplier_ID, (p, s) => new { p, s })
                    .Where(x => x.s.supplier_name == supplierName && x.p.Product_Status == "selling")
                    .Count();
            }
            catch
            {
                return 0;
            }
        }


    }


    // Model mới cho thông tin Series
    public class SeriesInfo
    {
        public int SeriesId { get; set; }
        public string SeriesName { get; set; }
        public int ProductCount { get; set; }
    }

}