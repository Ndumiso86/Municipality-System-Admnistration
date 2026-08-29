using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Municipality_System_Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    [Authorize(Roles = "DepartmentHead")]
    public class DepartmentHeadController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public ActionResult Dashboard()
        {
            var userId = User.Identity.GetUserId();

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(db));
            var roles = userManager.GetRoles(userId);
            var roleName = roles.FirstOrDefault() ?? "Department Head";

            ViewBag.RoleName = roleName;

            ViewBag.PendingRepairs = db.Assets.Count(a => a.RepairStatus == "Pending" && a.Status == "Pending Repair");
            ViewBag.ApprovedRepairs = db.Assets.Count(a => a.RepairStatus == "Approved");
            ViewBag.InProgressRepairs = db.Assets.Count(a => a.RepairStatus == "In Progress");
            ViewBag.CompletedRepairs = db.Assets.Count(a => a.RepairStatus == "Completed");
            ViewBag.TotalAssets = db.Assets.Count();
            ViewBag.TotalStaff = db.Staff.Count(s => s.IsActive);

            ViewBag.RecentRepairs = db.Assets
                .Where(a => a.RepairStatus == "Pending" && a.Status == "Pending Repair")
                .OrderByDescending(a => a.RepairRequestDate)
                .Take(10)
                .ToList();

            return View();
        }

        [HttpGet]
        public ActionResult PendingRepairs()
        {
            var repairs = db.Assets
                .Where(a => a.RepairStatus == "Pending" && a.Status == "Pending Repair")
                .OrderBy(a => a.RepairRequestDate)
                .ToList();

            return View(repairs);
        }

        [HttpGet]
        public ActionResult ApproveRepair(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("PendingRepairs");
            }

            if (asset.RepairStatus != "Pending")
            {
                TempData["Error"] = "This repair is not in pending status.";
                return RedirectToAction("PendingRepairs");
            }

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveRepair(int id, string ApprovalNotes)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("PendingRepairs");
            }


            asset.RepairStatus = "Approved";
            asset.Status = "Approved"; 
            asset.LastUpdated = DateTime.Now;

            if (!string.IsNullOrEmpty(ApprovalNotes))
            {
                asset.Notes = (asset.Notes ?? "") + "\n" +
                    DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                    " - Department Head Approval: " + ApprovalNotes;
            }

            db.SaveChanges();

            var verifyAsset = db.Assets.Find(id);
            System.Diagnostics.Debug.WriteLine($"=== APPROVE REPAIR ===");
            System.Diagnostics.Debug.WriteLine($"Asset ID: {verifyAsset.AssetId}");
            System.Diagnostics.Debug.WriteLine($"Asset Name: {verifyAsset.AssetName}");
            System.Diagnostics.Debug.WriteLine($"RepairStatus: {verifyAsset.RepairStatus}");
            System.Diagnostics.Debug.WriteLine($"Status: {verifyAsset.Status}");
            System.Diagnostics.Debug.WriteLine($"Assigned Technician: {verifyAsset.RepairAssignedToUserId}");

            var technician = db.Staff.FirstOrDefault(s => s.UserId == verifyAsset.RepairAssignedToUserId);
            TempData["Success"] = $"Repair request for '{asset.AssetName}' has been approved. Assigned to: {(technician?.FullName ?? "Unknown")}";
            return RedirectToAction("PendingRepairs");
        }

        [HttpGet]
        public ActionResult RejectRepair(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("PendingRepairs");
            }

            if (asset.RepairStatus != "Pending")
            {
                TempData["Error"] = "This repair is not in pending status.";
                return RedirectToAction("PendingRepairs");
            }

            return View(asset);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectRepair(int id, string RejectionReason)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("PendingRepairs");
            }

            asset.RepairRequested = false;
            asset.RepairStatus = null;
            asset.Status = "Available";
            asset.LastUpdated = DateTime.Now;

            if (!string.IsNullOrEmpty(RejectionReason))
            {
                asset.Notes = (asset.Notes ?? "") + "\n" +
                    DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                    " - Repair Request Rejected: " + RejectionReason;
            }

            db.SaveChanges();

            TempData["Success"] = $"Repair request for '{asset.AssetName}' has been rejected.";
            return RedirectToAction("PendingRepairs");
        }

        [HttpGet]
        public ActionResult RepairHistory()
        {
            var repairs = db.Assets
                .Where(a => a.RepairStatus == "Approved" || a.RepairStatus == "Completed")
                .OrderByDescending(a => a.RepairCompletedDate ?? a.LastUpdated)
                .ToList();

            return View(repairs);
        }

        [HttpGet]
        public ActionResult StaffReport()
        {
            var staff = db.Staff.Where(s => s.IsActive).OrderBy(s => s.FirstName).ToList();
            return View(staff);
        }

        [HttpGet]
        public ActionResult AssetReport()
        {
            var assets = db.Assets.OrderBy(a => a.AssetName).ToList();
            return View(assets);
        }

        [HttpGet]
        public ActionResult RepairReport()
        {
            var repairs = db.Assets
                .Where(a => a.RepairRequested == true)
                .OrderByDescending(a => a.RepairRequestDate)
                .ToList();

            return View(repairs);
        }

        [HttpGet]
        public ActionResult Reports()
        {
            ViewBag.TotalStaff = db.Staff.Count(s => s.IsActive);
            ViewBag.TotalAssets = db.Assets.Count();
            ViewBag.TotalRepairs = db.Assets.Count(a => a.RepairRequested == true);
            ViewBag.CompletedRepairs = db.Assets.Count(a => a.RepairStatus == "Approved" || a.RepairStatus == "Completed");
            ViewBag.PendingRepairs = db.Assets.Count(a => a.RepairStatus == "Pending");
            ViewBag.InProgressRepairs = db.Assets.Count(a => a.RepairStatus == "In Progress");
            ViewBag.ApprovedRepairs = db.Assets.Count(a => a.RepairStatus == "Approved");

            return View();
        }

        [HttpGet]
        public JsonResult GetPendingRepairCount()
        {
            var count = db.Assets.Count(a => a.RepairStatus == "Pending" && a.Status == "Pending Repair");
            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
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