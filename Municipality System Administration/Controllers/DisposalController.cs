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
            ViewBag.ApprovedCount = db.DisposalRequests.Count(r => r.Status == "Approved");
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
                .FirstOrDefault(r => r.AssetId == id && r.Status == "Pending");

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

            TempData["Success"] = $"Disposal request submitted for '{asset.AssetName}'. Awaiting Finance Officer approval.";
            return RedirectToAction("Index", "Assets");
        }

        [Authorize(Roles = "FinanceOfficer")]
        [HttpGet]
        public ActionResult FinanceReview(int id)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "This request has already been reviewed.";
                return RedirectToAction("Index");
            }

            return View(request);
        }

        [Authorize(Roles = "FinanceOfficer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinanceReview(int id, string FinanceReviewNotes, string Action, decimal? DisposalValue)
        {
            var request = db.DisposalRequests.Find(id);
            if (request == null)
            {
                TempData["Error"] = "Request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "This request has already been reviewed.";
                return RedirectToAction("Index");
            }

            request.FinanceReviewedByUserId = User.Identity.GetUserId();
            request.FinanceReviewDate = DateTime.Now;
            request.FinanceReviewNotes = FinanceReviewNotes;

            if (Action == "Approve")
            {
                request.Status = "Approved";
                request.DisposalValue = DisposalValue;

                var asset = db.Assets.Find(request.AssetId);
                if (asset != null)
                {
                    asset.Status = "Disposed";
                    asset.IsActive = false;
                    asset.LastUpdated = DateTime.Now;

                    asset.Notes = (asset.Notes ?? "") + "\n" +
                        DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                        " - Disposal Approved by Finance: " + User.Identity.Name +
                        " - Method: " + request.DisposalMethod +
                        (DisposalValue.HasValue ? " - Value: " + DisposalValue.Value.ToString("C2") : "") +
                        (string.IsNullOrEmpty(FinanceReviewNotes) ? "" : " - Notes: " + FinanceReviewNotes);
                }

                db.SaveChanges();

                TempData["Success"] = $"Disposal request #{request.DisposalRequestId} has been approved by Finance. Asset '{asset?.AssetName}' has been disposed.";
            }
            else
            {
                request.Status = "Rejected";
                db.SaveChanges();

                TempData["Success"] = $"Disposal request #{request.DisposalRequestId} has been rejected by Finance.";
            }

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

    }
}