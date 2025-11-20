using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlyPhone.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View("Error404");
        }

        // GET: Error/Forbidden
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;
            return View("Error403");
        }

        // GET: Error/InternalError
        public ActionResult InternalError()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            return View("Error500");
        }

        // GET: Error/Index
        public ActionResult Index(int? statusCode)
        {
            if (statusCode.HasValue)
            {
                Response.StatusCode = statusCode.Value;
                Response.TrySkipIisCustomErrors = true;

                switch (statusCode.Value)
                {
                    case 404:
                        return View("Error404");
                    case 403:
                        return View("Error403");
                    case 500:
                        return View("Error500");
                    default:
                        return View("Error");
                }
            }

            return View("Error");
        }
    }
}