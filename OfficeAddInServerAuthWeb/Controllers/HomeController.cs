using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using OfficeAddInServerAuth.Helpers;
using OfficeAddInServerAuth.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace OfficeAddInServerAuth.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var userAuthStateId = Settings.GetUserAuthStateId(ControllerContext.HttpContext);
            if (Data.GetUserSessionToken(userAuthStateId) != null)
            {
                return RedirectToAction("Index", "Message");
            }
            ViewBag.StateKey = userAuthStateId;
            return View();
        }


        
    }
}