using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OnlyPhone.Models;

namespace OnlyPhone.Controllers
{
    public class ManageController : Controller
    {
        SQLDataClassesDataContext da = new SQLDataClassesDataContext();
        Xuly xl = new Xuly();
        // GET: Manage
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult User()
        {
            List<User> users = da.Users.ToList();
            return View(users);
        }
        public ActionResult Products()
        {
            return View();
        }
        public ActionResult Setting()
        {
            return View();
        }
        public ActionResult Stats()
        {
            return View();
        }
        
        public ActionResult Product_Manager()
        {
            List<Product_Infomation> pr = xl.itemproduct();
            return View(pr);
        }
    }
}