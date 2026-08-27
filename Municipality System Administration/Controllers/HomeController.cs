using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Municipality_System_Administration.Models;
using Sitecore.FakeDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AdminDashboard()
        {
            ViewBag.Title = "Admin Dashboard";

            ViewBag.TotalStaff = db.Staff.Count();
            ViewBag.ActiveStaff = db.Staff.Count(s => s.IsActive);
            ViewBag.TotalRoles = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(db)).Roles.Count();
            //ViewBag.TotalAssets = db.Assets?.Count() ?? 0;
            //ViewBag.TotalWorkOrders = db.WorkOrders?.Count() ?? 0;

            return View();
        }
    }
}