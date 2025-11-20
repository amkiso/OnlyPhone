using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace OnlyPhone.Models
{
    public class Product_Infomation
    {
        public string product_name { get; set; }
        public int product_id { get; set; }
        public List<string> product_description { get; set; }
        public string supplier_name { get; set; }
        public string product_status { get; set; }
        public decimal sale_price { get; set; }
        public int current_Quantity { get; set; }
        public string images { get; set; }

        // THAY ĐỔI: Thay type_product bằng series_name và thêm series_id
        public string series_name { get; set; }  // Tên dòng sản phẩm (VD: iPhone 15, Samsung Galaxy S24)
        public int series_id { get; set; }       // ID của series

        // Thêm các trường mới cho việc lọc và sắp xếp
        public decimal original_price { get; set; }
        public int total_sold { get; set; }
        public bool is_featured { get; set; }
        public bool is_new { get; set; }
        public DateTime created_date { get; set; }

        // Computed property để tính % giảm giá
        public decimal DiscountPercent
        {
            get
            {
                if (original_price > 0 && original_price > sale_price)
                    return Math.Round(((original_price - sale_price) / original_price) * 100);
                return 0;
            }
        }

        // Property kiểm tra còn hàng
        public bool IsInStock
        {
            get
            {
                return current_Quantity > 0 && product_status == "selling";
            }
        }
    }

    // ViewModel cho trang sản phẩm (đã cập nhật)
    public class ProductFilterViewModel
    {
        public List<Product_Infomation> Products { get; set; }

        // THAY ĐỔI: Thay TypeProductId thành SeriesId
        public int SeriesId { get; set; }

        public string CurrentSort { get; set; }
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public bool HasMore { get; set; }
        public int TotalProducts { get; set; }

        // THAY ĐỔI: Thay CategoryName thành SeriesName
        public string SeriesName { get; set; }
    }

    // ViewModel cho kết quả tìm kiếm
    public class SearchResultViewModel
    {
        public List<Product_Infomation> Products { get; set; }
        public string SearchTerm { get; set; }
        public int TotalResults { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // Model cho autocomplete search result (đã cập nhật)
    public class SearchResultItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal SalePrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public string ProductImage { get; set; }

        // THAY ĐỔI: Thay TypeProductName thành SeriesName
        public string SeriesName { get; set; }

        public decimal SimilarityScore { get; set; }
    }
}