using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OnlyPhone.Models;
using System.IO;

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

                // Tách chuỗi thành từng phần
                List<string> parts = description.Contains(";")
                    ? description.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(s => s.Trim())
                                 .ToList()
                    : new List<string> { description.Trim() };

                // Nếu danh sách rỗng thì trả về default
                if (parts.Count == 0)
                    return new List<string> { "Đang cập nhật thông tin..." };

                // ------- THÊM NHÃN CHO 2 PHẦN ĐẦU -------
                // 1. Màn hình:
                if (parts.Count >= 1)
                {
                    if (!parts[0].StartsWith("Màn hình", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[0] = "Màn hình: " + parts[0];
                    }
                }

                // 2. Kích cỡ:
                if (parts.Count >= 2)
                {
                    if (!parts[1].StartsWith("Kích", StringComparison.OrdinalIgnoreCase)
                        && !parts[1].StartsWith("Kích cỡ", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[1] = "Kích cỡ: " + parts[1];
                    }
                }

                return parts;
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
        public List<Product_Infomation> GetNewProducts(int count)
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
        public List<Product_Infomation> GetBestSellerProducts(int count)
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
        public List<Product_Infomation> GetDiscountProducts(int count)
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
        // Overload mới hỗ trợ lọc theo cả Supplier và Series
        public List<Product_Infomation> GetProductsBySupplier(string supplierName, string seriesName, int page = 1, int pageSize = 15)
        {
            try
            {
                // Sử dụng Query Syntax để dễ dàng join các bảng
                var query = from p in da.Products
                            join s in da.suppliers on p.supplier_ID equals s.supplier_ID
                            join ps in da.PhoneSeries on p.Series_id equals ps.Series_id // Join thêm bảng Series để lọc
                            where p.Product_Status == "selling"
                            select new { p, s, ps };

                // 1. Lọc theo Supplier (Nếu có)
                if (!string.IsNullOrEmpty(supplierName))
                {
                    query = query.Where(x => x.s.supplier_name == supplierName);
                }

                // 2. Lọc theo Series (Tham số mới thêm vào)
                if (!string.IsNullOrEmpty(seriesName))
                {
                    query = query.Where(x => x.ps.SeriesName == seriesName);
                }

                // 3. Sắp xếp
                query = query.OrderByDescending(x => x.p.Is_Featured)
                             .ThenByDescending(x => x.p.Created_Date);

                // 4. Phân trang & Lấy dữ liệu
                var resultList = query.Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .Select(x => new
                                      {
                                          Product = x.p,
                                          SupplierName = x.s.supplier_name,
                                          SeriesName = x.ps.SeriesName,
                                          // Tính tổng đã bán
                                          TotalSold = da.Orders_items
                                                        .Where(oi => oi.Product_ID == x.p.Product_ID)
                                                        .Sum(oi => (int?)oi.quantity) ?? 0
                                      }).ToList();

                // 5. Map sang Model Product_Infomation
                return resultList.Select(x => new Product_Infomation
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
                System.Diagnostics.Debug.WriteLine($"Error in GetProductsBySupplier (Overload): {ex.Message}");
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
        // =====================================================
        // CHECKOUT METHODS - THÊM VÀO CLASS Xuly (FIXED v2)
        // =====================================================

        /// <summary>
        /// Lấy thông tin checkout cho các sản phẩm đã chọn
        /// </summary>
        public CheckoutViewModel GetCheckoutInfo(int userId, List<int> productIds)
        {
            try
            {
                var model = new CheckoutViewModel();

                // 1. Lấy giỏ hàng
                var cart = da.shopping_carts.FirstOrDefault(c => c.ID_user == userId);
                if (cart == null)
                {
                    return model;
                }

                // 2. Lấy thông tin sản phẩm từ giỏ hàng
                var cartItems = (from ci in da.cart_items
                                 where ci.cart_ID == cart.cart_ID && productIds.Contains(ci.Product_ID)
                                 join p in da.Products on ci.Product_ID equals p.Product_ID
                                 select new CheckoutItem
                                 {
                                     ProductId = p.Product_ID,
                                     ProductName = p.Product_name,
                                     ImageUrl = p.Product_Image,
                                     Price = ci.Sale_Price ?? p.Sale_Price ?? 0,
                                     Quantity = ci.quantity ?? 1,
                                     AvailableStock = p.Current_Quantity
                                 }).ToList();

                model.Items = cartItems;
                model.SubTotal = cartItems.Sum(x => x.Total);

                // 3. Lấy thông tin người dùng
                model.UserInfo = GetUserShippingInfo(userId);

                // 4. Lấy danh sách voucher khả dụng
                model.AvailableVouchers = GetAvailableVouchers(userId, model.SubTotal);

                // 5. Lấy phương thức thanh toán
                model.PaymentMethods = GetPaymentMethods();
                model.SelectedPaymentMethod = "COD";

                // 6. Lấy phương thức vận chuyển
                model.ShippingMethods = GetShippingMethods();
                model.SelectedShippingMethod = "Standard";
                model.ShippingFee = model.ShippingMethods.FirstOrDefault(s => s.Code == "Standard")?.Fee ?? 30000;

                // 7. Tính tổng tiền
                model.TotalAmount = model.SubTotal + model.ShippingFee - model.DiscountAmount;

                return model;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetCheckoutInfo: {ex.Message}");
                return new CheckoutViewModel();
            }
        }

        /// <summary>
        /// Lấy thông tin giao hàng của user
        /// </summary>
        public UserShippingInfo GetUserShippingInfo(int userId)
        {
            try
            {
                var userDetail = da.User_details.FirstOrDefault(u => u.ID_user == userId);
                var user = da.Users.FirstOrDefault(u => u.ID_user == userId);

                if (userDetail != null)
                {
                    return new UserShippingInfo
                    {
                        RecipientName = userDetail.full_name ?? "",
                        PhoneNumber = userDetail.user_phone_number ?? "",
                        Email = user?.user_email ?? "",
                        Province = userDetail.Province ?? "",
                        Ward = userDetail.Ward ?? "",
                        AddressDetail = userDetail.user_address ?? ""
                    };
                }

                return new UserShippingInfo
                {
                    Email = user?.user_email ?? ""
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserShippingInfo: {ex.Message}");
                return new UserShippingInfo();
            }
        }

        /// <summary>
        /// Lấy danh sách voucher khả dụng
        /// </summary>
        public List<VoucherInfo> GetAvailableVouchers(int userId, decimal orderValue)
        {
            try
            {
                var now = DateTime.Now;

                return da.Vouchers
                    .Where(v => v.IsActive == true
                             && v.StartDate <= now
                             && v.EndDate >= now
                             && v.Quantity > v.QuantityUsed
                             && (v.MinOrderValue == null || v.MinOrderValue <= orderValue))
                    .Select(v => new VoucherInfo
                    {
                        VoucherId = v.VoucherID,
                        Code = v.Code,
                        Description = v.Descriptions,
                        DiscountType = v.DiscountType,
                        DiscountValue = v.DiscountValue,
                        MaxDiscountAmount = v.MaxDiscountAmount,
                        MinOrderValue = v.MinOrderValue ?? 0,
                        EndDate = v.EndDate,
                        RemainingQuantity = v.Quantity - v.QuantityUsed
                    })
                    .OrderByDescending(v => v.DiscountValue)
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAvailableVouchers: {ex.Message}");
                return new List<VoucherInfo>();
            }
        }

        /// <summary>
        /// Lấy danh sách phương thức thanh toán
        /// </summary>
        public List<PaymentMethodInfo> GetPaymentMethods()
        {
            return new List<PaymentMethodInfo>
    {
        new PaymentMethodInfo
        {
            Code = "COD",
            Name = "Thanh toán khi nhận hàng (COD)",
            Description = "Thanh toán bằng tiền mặt khi nhận hàng",
            Icon = "fa-money-bill-wave",
            IsAvailable = true
        },
        new PaymentMethodInfo
        {
            Code = "BankTransfer",
            Name = "Chuyển khoản ngân hàng",
            Description = "Chuyển khoản qua số tài khoản ngân hàng",
            Icon = "fa-university",
            IsAvailable = true
        },
        new PaymentMethodInfo
        {
            Code = "Momo",
            Name = "Ví MoMo",
            Description = "Thanh toán qua ví điện tử MoMo",
            Icon = "fa-wallet",
            IsAvailable = true
        },
        new PaymentMethodInfo
        {
            Code = "ZaloPay",
            Name = "ZaloPay",
            Description = "Thanh toán qua ví điện tử ZaloPay",
            Icon = "fa-wallet",
            IsAvailable = true
        }
    };
        }

        /// <summary>
        /// Lấy danh sách phương thức vận chuyển
        /// </summary>
        public List<ShippingMethodInfo> GetShippingMethods()
        {
            return new List<ShippingMethodInfo>
    {
        new ShippingMethodInfo
        {
            Code = "Standard",
            Name = "Giao hàng tiêu chuẩn",
            Description = "Nhận hàng trong 3-5 ngày",
            Fee = 30000,
            EstimatedDays = 4
        },
        new ShippingMethodInfo
        {
            Code = "Fast",
            Name = "Giao hàng nhanh",
            Description = "Nhận hàng trong 1-2 ngày",
            Fee = 50000,
            EstimatedDays = 2
        },
        new ShippingMethodInfo
        {
            Code = "Express",
            Name = "Giao hàng hỏa tốc",
            Description = "Nhận hàng trong 24 giờ",
            Fee = 80000,
            EstimatedDays = 1
        }
    };
        }

        /// <summary>
        /// Tính phí vận chuyển theo phương thức
        /// </summary>
        public decimal CalculateShippingFee(string shippingMethod)
        {
            var method = GetShippingMethods().FirstOrDefault(s => s.Code == shippingMethod);
            return method?.Fee ?? 30000;
        }

        /// <summary>
        /// Tính số tiền giảm giá từ voucher
        /// </summary>
        public decimal CalculateDiscount(int voucherId, decimal orderValue)
        {
            try
            {
                var voucher = da.Vouchers.FirstOrDefault(v => v.VoucherID == voucherId);

                if (voucher == null || !voucher.IsActive || voucher.Quantity <= voucher.QuantityUsed)
                    return 0;

                if (voucher.MinOrderValue.HasValue && orderValue < voucher.MinOrderValue.Value)
                    return 0;

                var now = DateTime.Now;
                if (now < voucher.StartDate || now > voucher.EndDate)
                    return 0;

                decimal discount = 0;

                if (voucher.DiscountType == "PERCENT")
                {
                    discount = orderValue * voucher.DiscountValue / 100;
                    if (voucher.MaxDiscountAmount.HasValue && discount > voucher.MaxDiscountAmount.Value)
                        discount = voucher.MaxDiscountAmount.Value;
                }
                else if (voucher.DiscountType == "AMOUNT")
                {
                    discount = voucher.DiscountValue;
                }

                return discount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalculateDiscount: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của đơn hàng trước khi đặt
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateOrder(int userId, List<int> productIds)
        {
            try
            {
                // Kiểm tra giỏ hàng
                var cart = da.shopping_carts.FirstOrDefault(c => c.ID_user == userId);
                if (cart == null)
                    return (false, "Không tìm thấy giỏ hàng");

                // Kiểm tra sản phẩm
                var cartItems = da.cart_items
                    .Where(ci => ci.cart_ID == cart.cart_ID && productIds.Contains(ci.Product_ID))
                    .ToList();

                if (!cartItems.Any())
                    return (false, "Không có sản phẩm nào được chọn");

                // Kiểm tra tồn kho
                foreach (var item in cartItems)
                {
                    var product = da.Products.FirstOrDefault(p => p.Product_ID == item.Product_ID);
                    if (product == null)
                        return (false, $"Sản phẩm ID {item.Product_ID} không tồn tại");

                    if (product.Current_Quantity < item.quantity)
                        return (false, $"Sản phẩm {product.Product_name} không đủ số lượng trong kho");

                    if (product.Product_Status != "Selling")
                        return (false, $"Sản phẩm {product.Product_name} hiện không khả dụng");
                }

                return (true, "");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ValidateOrder: {ex.Message}");
                return (false, "Lỗi kiểm tra đơn hàng: " + ex.Message);
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới
        /// </summary>
        public OrderResult CreateOrder(int userId, CheckoutRequest request)
        {
            try
            {
                // 1. Validate đơn hàng
                var validation = ValidateOrder(userId, request.ProductIds);
                if (!validation.IsValid)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = validation.ErrorMessage,
                        ErrorCode = "VALIDATION_ERROR"
                    };
                }

                // 2. Tính toán các giá trị
                var cart = da.shopping_carts.FirstOrDefault(c => c.ID_user == userId);
                var cartItems = da.cart_items
                    .Where(ci => ci.cart_ID == cart.cart_ID && request.ProductIds.Contains(ci.Product_ID))
                    .ToList();

                decimal subTotal = cartItems.Sum(ci => (ci.Sale_Price ?? 0) * (ci.quantity ?? 1));
                decimal shippingFee = CalculateShippingFee(request.ShippingMethod);
                decimal discountAmount = request.VoucherId.HasValue
                    ? CalculateDiscount(request.VoucherId.Value, subTotal)
                    : 0;
                decimal totalAmount = subTotal + shippingFee - discountAmount;

                // 3. Tạo Order ID (max 10 chars: ORDxxxxxx)
                // Lấy số đơn hàng cuối cùng để tạo số tuần tự
                int nextOrderNumber = 1;
                var lastOrder = da.Orders.OrderByDescending(o => o.Order_ID).FirstOrDefault();
                if (lastOrder != null && lastOrder.Order_ID.StartsWith("ORD"))
                {
                    int.TryParse(lastOrder.Order_ID.Substring(3), out nextOrderNumber);
                    nextOrderNumber++;
                }
                string orderId = "ORD" + nextOrderNumber.ToString("000000"); // ORD000001

                // 4. Tạo đơn hàng
                var order = new Order
                {
                    Order_ID = orderId,
                    ID_user = userId,
                    VoucherID = request.VoucherId,
                    Order_Date = DateTime.Now,
                    StatusID = 1,
                    Total_Amount = totalAmount
                };
                da.Orders.InsertOnSubmit(order);

                // 5. Tạo chi tiết đơn hàng (Order_item_ID max 10 chars)
                int itemIndex = 1;
                foreach (var cartItem in cartItems)
                {
                    var orderItem = new Orders_item
                    {
                        Order_item_ID = orderId.Replace("ORD", "OI") + itemIndex.ToString("00"), // OI00000101
                        Order_ID = orderId,
                        Product_ID = cartItem.Product_ID,
                        quantity = cartItem.quantity,
                        Sale_Price = cartItem.Sale_Price
                    };
                    da.Orders_items.InsertOnSubmit(orderItem);
                    itemIndex++;
                }

                // 6. Cập nhật tồn kho
                foreach (var cartItem in cartItems)
                {
                    var product = da.Products.FirstOrDefault(p => p.Product_ID == cartItem.Product_ID);
                    if (product != null)
                    {
                        product.Current_Quantity -= (int)cartItem.quantity;
                    }
                }

                // 7. Cập nhật voucher nếu có
                if (request.VoucherId.HasValue)
                {
                    var voucher = da.Vouchers.FirstOrDefault(v => v.VoucherID == request.VoucherId.Value);
                    if (voucher != null)
                    {
                        voucher.QuantityUsed++;
                    }
                }

                // 8. Tạo thông tin giao hàng
                var shipping = new Shipping
                {
                    Order_ID = orderId,
                    Recipient_Name = request.ShippingInfo.RecipientName,
                    Phone_Number = request.ShippingInfo.PhoneNumber,
                    Email = request.ShippingInfo.Email,
                    Province = request.ShippingInfo.Province,
                    District = request.ShippingInfo.District,
                    Ward = request.ShippingInfo.Ward,
                    Address_Detail = request.ShippingInfo.AddressDetail,
                    Full_Address = request.ShippingInfo.FullAddress,
                    Shipping_Method = request.ShippingMethod,
                    Shipping_Fee = shippingFee,
                    Estimated_Delivery_Date = DateTime.Now.AddDays(GetShippingMethods()
                        .FirstOrDefault(s => s.Code == request.ShippingMethod)?.EstimatedDays ?? 3),
                    Shipping_Status = "Pending",
                    Notes = request.ShippingInfo.Notes,
                    Created_At = DateTime.Now
                };
                da.Shippings.InsertOnSubmit(shipping);

                // 9. Tạo payment (PaymentID max 10 chars)
                string paymentId = orderId.Replace("ORD", "PAY"); // PAY000001
                var payment = new Payment
                {
                    PaymentID = paymentId,
                    cart_ID = cart.cart_ID,
                    Order_ID = orderId,
                    Payment_Method = request.PaymentMethod,
                    Payment_Status = request.PaymentMethod == "COD" ? "Chờ thanh toán" : "Đã thanh toán",
                    Payment_Date = DateTime.Now,
                    Amount = totalAmount
                };
                da.Payments.InsertOnSubmit(payment);

                // 10. Xóa sản phẩm khỏi giỏ hàng
                foreach (var item in cartItems)
                {
                    da.cart_items.DeleteOnSubmit(item);
                }

                // 11. Submit changes
                da.SubmitChanges();

                return new OrderResult
                {
                    Success = true,
                    Message = "Đặt hàng thành công",
                    OrderId = orderId,
                    TotalAmount = totalAmount
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CreateOrder: {ex.Message}");
                return new OrderResult
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đặt hàng: " + ex.Message,
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// Lấy thông tin đơn hàng để hiển thị trang xác nhận
        /// </summary>
        public OrderConfirmationViewModel GetOrderConfirmation(string orderId)
        {
            try
            {
                var order = da.Orders.FirstOrDefault(o => o.Order_ID == orderId);
                if (order == null)
                    return null;

                var shipping = da.Shippings.FirstOrDefault(s => s.Order_ID == orderId);
                var payment = da.Payments.FirstOrDefault(p => p.Order_ID == orderId);
                var status = da.OrderStatus.FirstOrDefault(s => s.StatusID == order.StatusID);

                var orderItems = (from oi in da.Orders_items
                                  where oi.Order_ID == orderId
                                  join p in da.Products on oi.Product_ID equals p.Product_ID
                                  select new CheckoutItem
                                  {
                                      ProductId = p.Product_ID,
                                      ProductName = p.Product_name,
                                      ImageUrl = p.Product_Image,
                                      Price = oi.Sale_Price ?? 0,
                                      Quantity = oi.quantity ?? 1
                                  }).ToList();

                // Lấy status name - thử nhiều cách
                string statusName = "Đang xử lý";
                if (status != null)
                {
                    try
                    {
                        // Cách 1: Thử trực tiếp các tên có thể
                        var statusType = status.GetType();

                        // Thử Status_Name
                        var prop = statusType.GetProperty("Status_Name");
                        if (prop != null)
                        {
                            statusName = prop.GetValue(status)?.ToString() ?? "Đang xử lý";
                        }
                        else
                        {
                            // Thử StatusName
                            prop = statusType.GetProperty("StatusName");
                            if (prop != null)
                            {
                                statusName = prop.GetValue(status)?.ToString() ?? "Đang xử lý";
                            }
                            else
                            {
                                // Thử Name
                                prop = statusType.GetProperty("Name");
                                if (prop != null)
                                {
                                    statusName = prop.GetValue(status)?.ToString() ?? "Đang xử lý";
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Nếu lỗi thì dùng giá trị mặc định
                        statusName = "Đang xử lý";
                    }
                }

                return new OrderConfirmationViewModel
                {
                    OrderId = orderId,
                    OrderDate = order.Order_Date ?? DateTime.Now,
                    OrderStatus = statusName,
                    TotalAmount = order.Total_Amount ?? 0,
                    PaymentMethod = payment?.Payment_Method ?? "",
                    PaymentStatus = payment?.Payment_Status ?? "",
                    RecipientName = shipping?.Recipient_Name ?? "",
                    PhoneNumber = shipping?.Phone_Number ?? "",
                    FullAddress = shipping?.Full_Address ?? "",
                    ShippingMethod = shipping?.Shipping_Method ?? "",
                    ShippingFee = shipping?.Shipping_Fee ?? 0,
                    EstimatedDeliveryDate = shipping?.Estimated_Delivery_Date,
                    Items = orderItems
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetOrderConfirmation: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Lấy thông tin profile của user
        /// </summary>
        public UserProfileViewModel GetUserProfile(int userId)
        {
            try
            {
                var user = da.Users.FirstOrDefault(u => u.ID_user == userId);
                if (user == null)
                    return null;

                var userDetail = da.User_details.FirstOrDefault(ud => ud.ID_user == userId);

                return new UserProfileViewModel
                {
                    UserId = userId,
                    FullName = userDetail?.full_name ?? "Chưa cập nhật",
                    Email = user.user_email,
                    PhoneNumber = userDetail?.user_phone_number ?? "Chưa cập nhật",
                    Province = userDetail?.Province,
                    Ward = userDetail?.Ward,
                    AddressDetail = userDetail?.user_address,
                    AvatarUrl = userDetail?.user_pic ?? "noAvt.jpg",
                    CreatedDate = userDetail?.date_create,
                    LastModified = userDetail?.last_change,
                    IsActive = userDetail?.user_status ?? false,
                    RoleType = user.user_type,
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserProfile: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Cập nhật thông tin profile
        /// </summary>
        public UpdateProfileResult UpdateUserProfile(EditProfileRequest request)
        {
            try
            {
                var userDetail = da.User_details.FirstOrDefault(ud => ud.ID_user == request.UserId);

                if (userDetail == null)
                {
                    return new UpdateProfileResult
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin người dùng",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                // Cập nhật thông tin
                userDetail.full_name = request.FullName;
                userDetail.user_phone_number = request.PhoneNumber;
                userDetail.Province = request.Province;
                userDetail.Ward = request.Ward;
                userDetail.user_address = request.AddressDetail;
                // last_change sẽ tự động update bởi trigger

                da.SubmitChanges();

                return new UpdateProfileResult
                {
                    Success = true,
                    Message = "Cập nhật thông tin thành công"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateUserProfile: {ex.Message}");
                return new UpdateProfileResult
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi cập nhật thông tin: " + ex.Message,
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// Upload avatar với tên file unique
        /// </summary>
        public AvatarUploadResult UploadAvatar(int userId, HttpPostedFileBase file, string serverPath)
        {
            try
            {
                // Validate file
                if (file == null || file.ContentLength == 0)
                {
                    return new AvatarUploadResult
                    {
                        Success = false,
                        Message = "Vui lòng chọn file ảnh"
                    };
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new AvatarUploadResult
                    {
                        Success = false,
                        Message = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif)"
                    };
                }

                // Validate file size (max 5MB)
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    return new AvatarUploadResult
                    {
                        Success = false,
                        Message = "Kích thước file không được vượt quá 5MB"
                    };
                }

                // Tạo tên file unique: userId_timestamp_random.extension
                // VD: 123_20251122151030_a3f2b1.jpg
                string uniqueFileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 6)}{fileExtension}";

                // Đường dẫn đầy đủ
                string avatarFolder = Path.Combine(serverPath, "Content", "Pic", "Avartar");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(avatarFolder))
                {
                    Directory.CreateDirectory(avatarFolder);
                }

                string fullPath = Path.Combine(avatarFolder, uniqueFileName);

                // Xóa avatar cũ nếu có (trừ noAvt.jpg)
                var userDetail = da.User_details.FirstOrDefault(ud => ud.ID_user == userId);
                if (userDetail != null && !string.IsNullOrEmpty(userDetail.user_pic) && userDetail.user_pic != "noAvt.jpg")
                {
                    string oldAvatarPath = Path.Combine(avatarFolder, userDetail.user_pic);
                    if (File.Exists(oldAvatarPath))
                    {
                        try
                        {
                            File.Delete(oldAvatarPath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deleting old avatar: {ex.Message}");
                        }
                    }
                }

                // Save file
                file.SaveAs(fullPath);

                // Cập nhật database
                if (userDetail != null)
                {
                    userDetail.user_pic = uniqueFileName;
                    da.SubmitChanges();
                }

                return new AvatarUploadResult
                {
                    Success = true,
                    Message = "Upload ảnh đại diện thành công",
                    FileName = uniqueFileName,
                    FilePath = $"~/Content/Pic/Avartar/{uniqueFileName}",
                    FileSize = file.ContentLength
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UploadAvatar: {ex.Message}");
                return new AvatarUploadResult
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi upload ảnh: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public UpdateProfileResult ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                var user = da.Users.FirstOrDefault(u => u.ID_user == request.UserId);

                if (user == null)
                {
                    return new UpdateProfileResult
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng",
                        ErrorCode = "USER_NOT_FOUND"
                    };
                }

                // Verify current password
                string hashedCurrentPassword = HashPassword(request.CurrentPassword);
                if (user.user_password != hashedCurrentPassword)
                {
                    return new UpdateProfileResult
                    {
                        Success = false,
                        Message = "Mật khẩu hiện tại không đúng",
                        ErrorCode = "WRONG_PASSWORD"
                    };
                }

                // Update new password
                user.user_password = HashPassword(request.NewPassword);
                da.SubmitChanges();

                return new UpdateProfileResult
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ChangePassword: {ex.Message}");
                return new UpdateProfileResult
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi đổi mật khẩu: " + ex.Message,
                    ErrorCode = "SYSTEM_ERROR"
                };
            }
        }

        /// <summary>
        /// Kiểm tra email đã tồn tại chưa (dùng cho validation)
        /// </summary>
        public bool IsEmailExists(string email, int? excludeUserId = null)
        {
            try
            {
                var query = da.Users.Where(u => u.user_email == email);

                if (excludeUserId.HasValue)
                {
                    query = query.Where(u => u.ID_user != excludeUserId.Value);
                }

                return query.Any();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in IsEmailExists: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra số điện thoại đã tồn tại chưa
        /// </summary>
        public bool IsPhoneExists(string phone, int? excludeUserId = null)
        {
            try
            {
                var query = da.User_details.Where(ud => ud.user_phone_number == phone);

                if (excludeUserId.HasValue)
                {
                    query = query.Where(ud => ud.ID_user != excludeUserId.Value);
                }

                return query.Any();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in IsPhoneExists: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của user (cho trang Order History)
        /// </summary>
        public List<OrderHistoryItem> GetUserOrderHistory(int userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var orders = (from o in da.Orders
                              where o.ID_user == userId
                              join s in da.OrderStatus on o.StatusID equals s.StatusID
                              orderby o.Order_Date descending
                              select new
                              {
                                  Order = o,
                                  StatusName = s.StatusName ?? s.StatusName ?? "Đang xử lý",
                                  ItemCount = da.Orders_items.Where(oi => oi.Order_ID == o.Order_ID).Count()
                              })
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

                return orders.Select(x => new OrderHistoryItem
                {
                    OrderId = x.Order.Order_ID,
                    OrderDate = x.Order.Order_Date ?? DateTime.Now,
                    TotalAmount = x.Order.Total_Amount ?? 0,
                    StatusName = x.StatusName,
                    ItemCount = x.ItemCount
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetUserOrderHistory: {ex.Message}");
                return new List<OrderHistoryItem>();
            }
        }

        // =====================================================
        // HELPER MODEL FOR ORDER HISTORY
        // =====================================================
        
    }

    public class OrderHistoryItem
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; }
        public int ItemCount { get; set; }
    }
    // Model mới cho thông tin Series
    public class SeriesInfo
    {
        public int SeriesId { get; set; }
        public string SeriesName { get; set; }
        public int ProductCount { get; set; }
    }

}