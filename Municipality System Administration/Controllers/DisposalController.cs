using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Municipality_System_Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    [Authorize]
    public class DisposalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Admin, AssetManager, FinanceOfficer")]
        [HttpGet]
        public ActionResult Index(string status)
        {
            var requests = db.DisposalRequests.AsQueryable();

            var user = User;
            if (user.IsInRole("FinanceOfficer"))
            {
                requests = requests.Where(r => r.Status == "Pending");
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                requests = requests.Where(r => r.Status == status);
            }

            var requestList = requests
                .OrderByDescending(r => r.RequestDate)
                .ToList();

            // Statistics
            ViewBag.TotalRequests = db.DisposalRequests.Count();
            ViewBag.PendingCount = db.DisposalRequests.Count(r => r.Status == "Pending");
            ViewBag.ReviewedCount = db.DisposalRequests.Count(r => r.Status == "Reviewed");
            ViewBag.ApprovedCount = db.DisposalRequests.Count(r => r.Status == "Approved");
            ViewBag.CompletedCount = db.DisposalRequests.Count(r => r.Status == "Completed");
            ViewBag.RejectedCount = db.DisposalRequests.Count(r => r.Status == "Rejected");
            ViewBag.SelectedStatus = status;

            return View(requestList);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public new ActionResult Request(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index", "Assets");
            }

            if (asset.Status == "Disposed")
            {
                TempData["Error"] = "This asset has already been disposed.";
                return RedirectToAction("Index", "Assets");
            }

            var existingRequest = db.DisposalRequests
                .FirstOrDefault(r => r.AssetId == id && r.Status != "Completed" && r.Status != "Rejected");

            if (existingRequest != null)
            {
                TempData["Error"] = "A disposal request already exists for this asset.";
                return RedirectToAction("Index", "Assets");
            }

            var disposalMethods = new SelectList(new List<string>
            {
                "Auction",
                "Scrap",
                "Donation",
                "Recycle",
                "Trade-in",
                "Write-off"
            });

            ViewBag.DisposalMethods = disposalMethods;
            ViewBag.Asset = asset;

            return View(asset);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Request(int id, string Reason, string DisposalMethod)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index", "Assets");
            }

            if (string.IsNullOrEmpty(Reason))
            {
                ModelState.AddModelError("", "Please provide a reason for disposal.");
                ViewBag.DisposalMethods = new SelectList(new List<string>
                {
                    "Auction", "Scrap", "Donation", "Recycle", "Trade-in", "Write-off"
                });
                ViewBag.Asset = asset;
                return View();
            }

            if (string.IsNullOrEmpty(DisposalMethod))
            {
                ModelState.AddModelError("", "Please select a disposal method.");
                ViewBag.DisposalMethods = new SelectList(new List<string>
                {
                    "Auction", "Scrap", "Donation", "Recycle", "Trade-in", "Write-off"
                });
                ViewBag.Asset = asset;
                return View();
            }

            var request = new DisposalRequest
            {
                AssetId = id,
                RequestedByUserId = User.Identity.GetUserId(),
                RequestDate = DateTime.Now,
                Reason = Reason,
                DisposalMethod = DisposalMethod,
                Status = "Pending"
            };

            db.DisposalRequests.Add(request);
            db.SaveChanges();

            asset.Notes = (asset.Notes ?? "") + "\n" +
                DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                " - Disposal Requested by: " + User.Identity.Name +
                " - Method: " + DisposalMethod +
                " - Reason: " + Reason;

            db.SaveChanges();

            TempData["Success"] = $"Disposal request submitted for '{asset.AssetName}'. Awaiting review.";
            return RedirectToAction("Index", "Assets");
        }

        [Authorize(Roles = "Admin, AssetManager, FinanceOfficer")]
        [HttpGet]
        public ActionResult Review(int id)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending" && request.Status != "Reviewed")
            {
                TempData["Error"] = "This request has already been reviewed.";
                return RedirectToAction("Index");
            }

            var disposalMethods = new SelectList(new List<string>
            {
                "Auction",
                "Scrap",
                "Donation",
                "Recycle",
                "Trade-in",
                "Write-off"
            });

            ViewBag.DisposalMethods = disposalMethods;

            return View(request);
        }

        [Authorize(Roles = "Admin, AssetManager, FinanceOfficer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review(int id, string ReviewNotes, string Action, string DisposalMethod, decimal? DisposalValue)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending" && request.Status != "Reviewed")
            {
                TempData["Error"] = "This request has already been reviewed.";
                return RedirectToAction("Index");
            }

            request.ReviewedByUserId = User.Identity.GetUserId();
            request.ReviewedDate = DateTime.Now;
            request.ReviewNotes = ReviewNotes;

            if (Action == "Approve")
            {
                if (string.IsNullOrEmpty(DisposalMethod))
                {
                    ModelState.AddModelError("", "Please select a disposal method.");
                    ViewBag.DisposalMethods = new SelectList(new List<string>
                    {
                        "Auction", "Scrap", "Donation", "Recycle", "Trade-in", "Write-off"
                    });
                    return View(request);
                }

                request.Status = "Approved";
                request.ApprovalDate = DateTime.Now;
                request.ApprovedByUserId = User.Identity.GetUserId();
                request.DisposalMethod = DisposalMethod;
                request.DisposalValue = DisposalValue;

                var asset = db.Assets.Find(request.AssetId);
                if (asset != null)
                {
                    asset.Status = "Disposed";
                    asset.IsActive = false;
                    asset.LastUpdated = DateTime.Now;

                    asset.Notes = (asset.Notes ?? "") + "\n" +
                        DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                        " - Disposal Approved by: " + User.Identity.Name +
                        " - Method: " + DisposalMethod +
                        (DisposalValue.HasValue ? " - Value: " + DisposalValue.Value.ToString("C2") : "") +
                        (string.IsNullOrEmpty(ReviewNotes) ? "" : " - Notes: " + ReviewNotes);
                }

                TempData["Success"] = $"Disposal request #{request.DisposalRequestId} has been approved. Method: {DisposalMethod}.";
            }
            else if (Action == "Reject")
            {
                request.Status = "Rejected";
                TempData["Success"] = $"Disposal request #{request.DisposalRequestId} has been rejected.";
            }
            else
            {
                request.Status = "Reviewed";
                TempData["Success"] = $"Disposal request #{request.DisposalRequestId} has been reviewed. Awaiting final approval.";
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Complete(int id)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Approved")
            {
                TempData["Error"] = "This request must be approved before it can be completed.";
                return RedirectToAction("Index");
            }

            return View(request);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Complete(int id, string CompletionNotes)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            request.Status = "Completed";
            request.CompletionDate = DateTime.Now;
            request.CompletionNotes = CompletionNotes;

            var asset = db.Assets.Find(request.AssetId);
            if (asset != null)
            {
                asset.Notes = (asset.Notes ?? "") + "\n" +
                    DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                    " - Disposal Completed: " +
                    (string.IsNullOrEmpty(CompletionNotes) ? "" : " - " + CompletionNotes);
            }

            db.SaveChanges();

            TempData["Success"] = $"Disposal request #{request.DisposalRequestId} has been completed.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Cancel(int id)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "This request cannot be cancelled as it has already been reviewed.";
                return RedirectToAction("Index");
            }

            return View(request);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int id)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            db.DisposalRequests.Remove(request);
            db.SaveChanges();

            TempData["Success"] = $"Disposal request #{request.DisposalRequestId} has been cancelled.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            return View(request);
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