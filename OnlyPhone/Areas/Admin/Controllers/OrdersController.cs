using OnlyPhone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Admin/Order
      
            Xuly xl = new Xuly();
            SQLDataClassesDataContext da = new SQLDataClassesDataContext(); // Dùng để lấy list status cho dropdown

            // GET: Admin/Orders
            public ActionResult Index(int? status, string keyword, string from, string to)
            {
                DateTime? fromDate = null, toDate = null;
                if (!string.IsNullOrEmpty(from)) fromDate = DateTime.Parse(from);
                if (!string.IsNullOrEmpty(to)) toDate = DateTime.Parse(to);

                var orders = xl.GetOrders(status, keyword, fromDate, toDate);

                // Stats
                ViewBag.TotalOrders = da.Orders.Count();
                ViewBag.TotalPending = da.Orders.Count(o => o.StatusID == 1);
                ViewBag.TotalCancelled = da.Orders.Count(o => o.StatusID == 6);
                ViewBag.TotalCompleted = da.Orders.Count(o => o.StatusID == 5);

                // Dropdown Status List
                ViewBag.StatusList = da.OrderStatus.ToList();

                return View(orders);
            }

            [HttpPost]
            public JsonResult UpdateStatus(string orderId, int newStatus)
            {
                bool res = xl.UpdateOrderStatus(orderId, newStatus);
                return Json(new { success = res });
            }

            [HttpPost]
            public JsonResult RemoveProduct(string orderId, int productId)
            {
                bool res = xl.RemoveOrderItem(orderId, productId);
                return Json(new { success = res });
            }
        }
    }
