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
            //todo: add support for Google and other auth providers
            if (Data.GetUserSessionToken(userAuthStateId, Settings.AzureADAuthority) != null)
            {
                return RedirectToAction("Index", "Message");
            }
            ViewBag.StateKey = userAuthStateId;
            var tk = new SessionToken();
            return View(tk);
        }


        
    }
}