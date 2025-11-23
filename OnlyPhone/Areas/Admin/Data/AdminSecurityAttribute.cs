using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace On.Areas.Admin // Đặt namespace cho đúng chỗ bạn lưu file
{
    public class AdminSecurityAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // --- LOGIC KIỂM TRA ĐĂNG NHẬP ---
            // Bạn thay thế điều kiện này bằng logic thực tế của bạn
            // Ví dụ: kiểm tra Session, Cookie, hoặc Identity
           
            bool daDangNhap = (HttpContext.Current.Session["UserType"] == "Admin" || HttpContext.Current.Session["UserType"] != null);

            if (!daDangNhap)
            {
                // NẾU CHƯA ĐĂNG NHẬP -> CHUYỂN HƯỚNG VỀ 404

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Error" },
                        { "action", "NotFound" },
                        { "area", "" } // Quan trọng: Để trống nghĩa là tìm Controller ở trang chủ (Main), không phải trong Area Admin
                    }
                );
            }

            base.OnActionExecuting(filterContext);
        }
    }
}