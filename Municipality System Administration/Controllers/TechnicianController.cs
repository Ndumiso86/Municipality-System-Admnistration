using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Municipality_System_Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    [Authorize(Roles = "Technician")]
    public class TechnicianController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();
            var technician = db.Staff.FirstOrDefault(s => s.UserId == userId);

            ViewBag.ReadyToStart = db.Assets.Count(a => a.RepairAssignedToUserId == userId && a.RepairStatus == "Approved");
            ViewBag.InProgressRepairs = db.Assets.Count(a => a.RepairAssignedToUserId == userId && a.RepairStatus == "In Progress");
            ViewBag.CompletedRepairs = db.Assets.Count(a => a.RepairAssignedToUserId == userId && a.RepairStatus == "Completed");
            ViewBag.PendingRepairs = db.Assets.Count(a => a.RepairAssignedToUserId == userId && a.RepairStatus == "Pending");
            ViewBag.TotalAssigned = db.Assets.Count(a => a.RepairAssignedToUserId == userId);
            ViewBag.TechnicianName = technician?.FullName ?? "Technician";

            return View();
        }

        [HttpGet]
        public ActionResult RepairQueue()
        {
            var userId = User.Identity.GetUserId();

            var repairs = db.Assets
                .Where(a => a.RepairAssignedToUserId == userId &&
                           (a.RepairStatus == "Approved" || a.RepairStatus == "In Progress"))
                .OrderByDescending(a => a.RepairRequestDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"=== Technician {userId} Repair Queue ===");
            System.Diagnostics.Debug.WriteLine($"Total repairs found: {repairs.Count}");

            foreach (var r in repairs)
            {
                System.Diagnostics.Debug.WriteLine($"Asset: {r.AssetName} (ID: {r.AssetId})");
                System.Diagnostics.Debug.WriteLine($"  - RepairStatus: {r.RepairStatus}");
                System.Diagnostics.Debug.WriteLine($"  - Status: {r.Status}");
                System.Diagnostics.Debug.WriteLine($"---");
            }

            return View(repairs);
        }

        [HttpGet]
        public ActionResult RepairHistory()
        {
            var userId = User.Identity.GetUserId();

            var repairs = db.Assets
                .Where(a => a.RepairAssignedToUserId == userId &&
                           (a.RepairStatus == "Completed" || a.RepairStatus == "Approved"))
                .OrderByDescending(a => a.RepairCompletedDate ?? a.LastUpdated)
                .ToList();

            return View(repairs);
        }

        [HttpGet]
        public ActionResult StartRepair(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("RepairQueue");
            }

            if (asset.RepairStatus != "Approved")
            {
                TempData["Error"] = "This repair has not been approved by the Department Head yet. Current status: " + asset.RepairStatus;
                return RedirectToAction("RepairQueue");
            }

            if (asset.RepairAssignedToUserId != User.Identity.GetUserId())
            {
                TempData["Error"] = "You are not assigned to this repair.";
                return RedirectToAction("RepairQueue");
            }

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartRepair(int id, string RepairReport)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("RepairQueue");
            }

            if (asset.RepairStatus != "Approved")
            {
                TempData["Error"] = "This repair has not been approved by the Department Head yet.";
                return RedirectToAction("RepairQueue");
            }

            asset.RepairStatus = "In Progress";
            asset.Status = "In Repair";
            asset.LastUpdated = DateTime.Now;

            if (!string.IsNullOrEmpty(RepairReport))
            {
                asset.RepairReport = (asset.RepairReport ?? "") + "\n" +
                    DateTime.Now.ToString("dd MMM yyyy HH:mm") + " - Started repair: " + RepairReport;
            }

            db.SaveChanges();

            TempData["Success"] = $"Repair for '{asset.AssetName}' has been started.";
            return RedirectToAction("RepairQueue");
        }

        [HttpGet]
        public ActionResult CompleteRepair(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("RepairQueue");
            }

            if (asset.RepairStatus != "In Progress")
            {
                TempData["Error"] = "This repair is not in progress. Current status: " + asset.RepairStatus;
                return RedirectToAction("RepairQueue");
            }

            if (asset.RepairAssignedToUserId != User.Identity.GetUserId())
            {
                TempData["Error"] = "You are not assigned to this repair.";
                return RedirectToAction("RepairQueue");
            }

            return View(asset);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteRepair(int id, string RepairReport, decimal? RepairCost)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("RepairQueue");
            }

            if (string.IsNullOrEmpty(RepairReport))
            {
                ModelState.AddModelError("", "Please provide a repair report.");
                return View(asset);
            }

            asset.RepairStatus = "Completed";
            asset.RepairCompletedDate = DateTime.Now;
            asset.Status = "Repaired";
            asset.Condition = "Good";
            asset.LastMaintenanceDate = DateTime.Now;
            asset.LastUpdated = DateTime.Now;

            asset.RepairReport = (asset.RepairReport ?? "") + "\n" +
                DateTime.Now.ToString("dd MMM yyyy HH:mm") + " - Repair completed: " + RepairReport;

            if (RepairCost.HasValue && RepairCost.Value > 0)
            {
                asset.RepairCost = RepairCost.Value;
            }

            db.SaveChanges();

            TempData["Success"] = $"Repair for '{asset.AssetName}' completed successfully. Waiting for admin approval.";
            return RedirectToAction("RepairQueue");
        }

        [HttpGet]
        public ActionResult AssetDetails(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("RepairQueue");
            }

            if (asset.RepairAssignedToUserId != User.Identity.GetUserId())
            {
                TempData["Error"] = "You are not authorized to view this asset.";
                return RedirectToAction("RepairQueue");
            }

            return View(asset);
        }

        [HttpGet]
        public JsonResult GetRepairCount()
        {
            var userId = User.Identity.GetUserId();
            var count = db.Assets
                .Count(a => a.RepairAssignedToUserId == userId &&
                           a.RepairStatus == "Approved");

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult FixAssetStatuses()
        {
            var userId = User.Identity.GetUserId();
            var repairs = db.Assets.Where(a => a.RepairAssignedToUserId == userId).ToList();
            int fixedCount = 0;

            foreach (var asset in repairs)
            {
                if (asset.RepairStatus == "Approved" && asset.Status != "Approved")
                {
                    asset.Status = "Approved";
                    asset.LastUpdated = DateTime.Now;
                    fixedCount++;
                    System.Diagnostics.Debug.WriteLine($"Fixed asset {asset.AssetName}: Status set to Approved");
                }
                else if (asset.RepairStatus == "In Progress" && asset.Status != "In Repair")
                {
                    asset.Status = "In Repair";
                    asset.LastUpdated = DateTime.Now;
                    fixedCount++;
                    System.Diagnostics.Debug.WriteLine($"Fixed asset {asset.AssetName}: Status set to In Repair");
                }
                else if (asset.RepairStatus == "Completed" && asset.Status != "Repaired")
                {
                    asset.Status = "Repaired";
                    asset.LastUpdated = DateTime.Now;
                    fixedCount++;
                    System.Diagnostics.Debug.WriteLine($"Fixed asset {asset.AssetName}: Status set to Repaired");
                }
            }

            db.SaveChanges();
            TempData["Success"] = $"Fixed {fixedCount} asset statuses.";
            return RedirectToAction("RepairQueue");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}