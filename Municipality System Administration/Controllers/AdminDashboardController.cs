using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Municipality_System_Administration.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}