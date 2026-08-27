using Microsoft.AspNet.Identity;
using Municipality_System_Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        public ActionResult Index(int? assetId)
        {
            var today = DateTime.Now.Date;
            var thirtyDaysFromNow = today.AddDays(30);

            IQueryable<Asset> query = db.Assets.Where(a => a.IsActive);

            if (assetId.HasValue && assetId.Value > 0)
            {
                query = query.Where(a => a.AssetId == assetId.Value);
                var asset = db.Assets.Find(assetId.Value);
                ViewBag.AssetName = asset?.AssetName;
                ViewBag.AssetId = assetId.Value;
            }
            else
            {
                ViewBag.AssetName = "All Assets";
                ViewBag.AssetId = null;
            }

            var assets = query.ToList();

            var maintenanceList = assets.Select(a => new MaintenanceViewModel
            {
                AssetId = a.AssetId,
                AssetNumber = a.AssetNumber,
                AssetName = a.AssetName,
                Category = a.Category,
                Brand = a.Brand,
                Model = a.Model,
                LastMaintenanceDate = a.LastMaintenanceDate,
                NextMaintenanceDate = a.NextMaintenanceDate,
                MaintenanceFrequency = a.MaintenanceFrequency,
                MaintenanceNotes = a.MaintenanceNotes,
                Status = GetMaintenanceStatus(a.NextMaintenanceDate),
                IsActive = a.IsActive
            }).OrderBy(m => m.NextMaintenanceDate ?? DateTime.MaxValue).ToList();

            ViewBag.TotalAssets = assets.Count;
            ViewBag.UpcomingCount = maintenanceList.Count(m => m.Status == "Upcoming");
            ViewBag.OverdueCount = maintenanceList.Count(m => m.Status == "Overdue");
            ViewBag.ScheduledCount = maintenanceList.Count(m => m.NextMaintenanceDate.HasValue);
            ViewBag.CompletedCount = maintenanceList.Count(m => m.LastMaintenanceDate.HasValue);

            return View(maintenanceList);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Schedule(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult Schedule(int id, DateTime? NextMaintenanceDate, int? MaintenanceFrequency, string MaintenanceNotes)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (!NextMaintenanceDate.HasValue)
            {
                ModelState.AddModelError("", "Please select a maintenance date.");
                return View(asset);
            }

            asset.NextMaintenanceDate = NextMaintenanceDate;
            asset.MaintenanceFrequency = MaintenanceFrequency;
            asset.MaintenanceNotes = MaintenanceNotes;
            asset.LastUpdated = DateTime.Now;

            asset.Notes = (asset.Notes ?? "") + "\n" +
                DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                " - Maintenance Scheduled: " + NextMaintenanceDate.Value.ToString("dd MMM yyyy") +
                (MaintenanceFrequency.HasValue ? " (Every " + MaintenanceFrequency.Value + " months)" : "") +
                (string.IsNullOrEmpty(MaintenanceNotes) ? "" : " - " + MaintenanceNotes);

            db.SaveChanges();

            TempData["Success"] = $"Maintenance scheduled for '{asset.AssetName}' on {NextMaintenanceDate.Value.ToString("dd MMM yyyy")}.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager, Technician")]
        [HttpGet]
        public ActionResult Record(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            DateTime? suggestedNextDate = null;
            if (asset.MaintenanceFrequency.HasValue && asset.MaintenanceFrequency.Value > 0)
            {
                suggestedNextDate = DateTime.Now.AddMonths(asset.MaintenanceFrequency.Value);
            }
            else
            {
                suggestedNextDate = DateTime.Now.AddMonths(6); 
            }

            ViewBag.SuggestedNextDate = suggestedNextDate;
            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager, Technician")]
        public ActionResult Record(int id, DateTime? MaintenanceDate, DateTime? NextMaintenanceDate,
                                  string MaintenanceNotes, decimal? MaintenanceCost, string PerformedBy)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (!MaintenanceDate.HasValue)
            {
                ModelState.AddModelError("", "Please select a maintenance date.");
                ViewBag.SuggestedNextDate = DateTime.Now.AddMonths(6);
                return View(asset);
            }

            asset.LastMaintenanceDate = MaintenanceDate;
            asset.NextMaintenanceDate = NextMaintenanceDate;
            asset.MaintenanceNotes = MaintenanceNotes;
            asset.LastUpdated = DateTime.Now;

            asset.Notes = (asset.Notes ?? "") + "\n" +
                DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                " - Maintenance Recorded: " + MaintenanceDate.Value.ToString("dd MMM yyyy") +
                (string.IsNullOrEmpty(PerformedBy) ? "" : " by " + PerformedBy) +
                (MaintenanceCost.HasValue ? " (Cost: " + MaintenanceCost.Value.ToString("C2") + ")" : "") +
                (string.IsNullOrEmpty(MaintenanceNotes) ? "" : " - " + MaintenanceNotes);

            db.SaveChanges();

            TempData["Success"] = $"Maintenance recorded for '{asset.AssetName}' on {MaintenanceDate.Value.ToString("dd MMM yyyy")}.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            var history = new List<MaintenanceHistoryItem>();

            if (!string.IsNullOrEmpty(asset.Notes))
            {
                var lines = asset.Notes.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains("Maintenance Recorded:") || line.Contains("Maintenance Scheduled:"))
                    {
                        var datePart = "";
                        if (line.Contains(" - "))
                        {
                            var parts = line.Split(new[] { " - " }, StringSplitOptions.None);
                            if (parts.Length > 0)
                            {
                                datePart = parts[0];
                            }
                        }

                        DateTime parsedDate;
                        if (DateTime.TryParse(datePart, out parsedDate))
                        {
                            history.Add(new MaintenanceHistoryItem
                            {
                                Date = parsedDate,
                                Description = line.Trim(),
                                Type = line.Contains("Recorded") ? "Recorded" : "Scheduled"
                            });
                        }
                        else
                        {
                            history.Add(new MaintenanceHistoryItem
                            {
                                Date = DateTime.Now,
                                Description = line.Trim(),
                                Type = line.Contains("Recorded") ? "Recorded" : "Scheduled"
                            });
                        }
                    }
                }
            }

            ViewBag.MaintenanceHistory = history.OrderByDescending(h => h.Date).ToList();
            return View(asset);
        }

        private string GetMaintenanceStatus(DateTime? nextDate)
        {
            if (!nextDate.HasValue)
                return "Not Scheduled";

            var today = DateTime.Now.Date;
            var next = nextDate.Value.Date;

            if (next < today)
                return "Overdue";
            else if (next <= today.AddDays(30))
                return "Upcoming";
            else
                return "OK";
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