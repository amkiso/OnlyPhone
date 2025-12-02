using System;
using System.Collections.Generic;

namespace OnlyPhone.Areas.Admin.Data
{
    public class BrandViewModel
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Logo { get; set; }

        // Thống kê
        public int TotalProducts { get; set; }
        public int TotalSold { get; set; }

        // Danh sách dòng sản phẩm thuộc thương hiệu
        public List<SeriesDTO> SeriesList { get; set; }

        public BrandViewModel()
        {
            SeriesList = new List<SeriesDTO>();
        }
    }

    public class SeriesDTO
    {
        public int SeriesId { get; set; }
        public string SeriesName { get; set; }
        public int ProductCount { get; set; }
    }

    public class BrandStatsModel
    {
        public int TotalBrands { get; set; }
        public int TotalSeries { get; set; }
        public string BestSellingBrand { get; set; }
        public int BestSellingQuantity { get; set; }
    }
}