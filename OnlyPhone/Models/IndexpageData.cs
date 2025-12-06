using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlyPhone.Models
{
    public class IndexpageData
    {
        public List<slide> SlideImages { get; set; }

        // Các danh sách sản phẩm theo danh mục
        public List<Product_Infomation> FeaturedProducts { get; set; }      // Sản phẩm nổi bật
        public List<Product_Infomation> NewProducts { get; set; }            // Sản phẩm mới
        public List<Product_Infomation> BestSellerProducts { get; set; }     // Bán chạy nhất
        public List<Product_Infomation> DiscountProducts { get; set; }       // Giảm giá sốc

        // Danh sách sản phẩm theo thương hiệu
        public BrandProducts AppleProducts { get; set; }
        public BrandProducts SamsungProducts { get; set; }
        public BrandProducts XiaomiProducts { get; set; }
        public BrandProducts OppoProducts { get; set; }
        public BrandProducts VivoProducts { get; set; }
        public BrandProducts RealmeProducts { get; set; }

        // Danh sách tất cả series (cho menu brand)
        public List<SeriesInfo> AllSeries { get; set; }
        public List<NotificationInfo> Notifications { get; set; }
        public int UnreadNotificationCount { get; set; }
        // Constructor khởi tạo giá trị mặc định
        public IndexpageData()
        {
            SlideImages = new List<slide>();
            FeaturedProducts = new List<Product_Infomation>();
            NewProducts = new List<Product_Infomation>();
            BestSellerProducts = new List<Product_Infomation>();
            DiscountProducts = new List<Product_Infomation>();
            AllSeries = new List<SeriesInfo>();

            AppleProducts = new BrandProducts();
            SamsungProducts = new BrandProducts();
            XiaomiProducts = new BrandProducts();
            OppoProducts = new BrandProducts();
            VivoProducts = new BrandProducts();
            RealmeProducts = new BrandProducts();
        }
    }

    /// <summary>
    /// Class chứa sản phẩm theo thương hiệu
    /// </summary>
    public class BrandProducts
    {
        public string BrandName { get; set; }
        public int SeriesId { get; set; }
        public List<Product_Infomation> Products { get; set; }
        public int TotalProducts { get; set; }

        public BrandProducts()
        {
            Products = new List<Product_Infomation>();
        }
    }
}
