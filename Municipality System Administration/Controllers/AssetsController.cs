using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Municipality_System_Administration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    [Authorize]
    public class AssetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public static List<string> Categories = new List<string>
        {
            "Computer",
            "Laptop",
            "Printer",
            "Vehicle",
            "Office Furniture",
            "Generator",
            "Air Conditioner",
            "Projector",
            "Network Equipment",
            "Mobile Device",
            "Electrical Equipment",
            "Tool",
            "Building Equipment",
            "Furniture",
            "Medical Equipment",
            "Other"
        };

        public static List<string> Conditions = new List<string>
        {
            "New",
            "Excellent",
            "Good",
            "Fair",
            "Poor",
            "Damaged",
            "For Repair"
        };

        public static List<string> Statuses = new List<string>
        {
            "Available",
            "Assigned",
            "Pending Repair",
            "In Repair",
            "Repaired",
            "Disposed",
            "Lost",
            "Stolen"
        };

        public static List<string> Locations = new List<string>
        {
            "Head Office",
            "Warehouse A",
            "Warehouse B",
            "Workshop",
            "Site A",
            "Site B",
            "Site C",
            "Storage Room",
            "Other"
        };



        [HttpGet]
        public ActionResult Index(string search, string category, string status, string location)
        {
            var assets = db.Assets.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                assets = assets.Where(a =>
                    a.AssetName.ToLower().Contains(search) ||
                    a.AssetNumber.ToLower().Contains(search) ||
                    a.Brand.ToLower().Contains(search) ||
                    a.Model.ToLower().Contains(search) ||
                    a.SerialNumber.ToLower().Contains(search) ||
                    a.Supplier.ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                assets = assets.Where(a => a.Category == category);
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                assets = assets.Where(a => a.Status == status);
            }

            if (!string.IsNullOrEmpty(location) && location != "All")
            {
                assets = assets.Where(a => a.Location == location);
            }

            ViewBag.Categories = new SelectList(Categories);
            ViewBag.Statuses = new SelectList(Statuses);
            ViewBag.Locations = new SelectList(Locations);
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedLocation = location;
            ViewBag.SearchTerm = search;

            ViewBag.TotalAssets = assets.Count();
            ViewBag.AvailableAssets = assets.Count(a => a.Status == "Available");
            ViewBag.AssignedAssets = assets.Count(a => a.Status == "Assigned");
            ViewBag.InRepair = assets.Count(a => a.Status == "Pending Repair" || a.Status == "In Repair");
            ViewBag.DisposedAssets = assets.Count(a => a.Status == "Disposed");

            if (User.Identity.IsAuthenticated)
            {
                ViewBag.CurrentUserId = User.Identity.GetUserId();
            }

            return View(assets.OrderBy(a => a.AssetName).ToList());
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(Categories);
            ViewBag.Conditions = new SelectList(Conditions);
            ViewBag.Statuses = new SelectList(Statuses);
            ViewBag.Locations = new SelectList(Locations);

            return View(new Asset
            {
                IsActive = true,
                Status = "Available",
                Condition = "New",
                DateCreated = DateTime.Now,
                PurchaseDate = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult Create(Asset asset)
        {
            ModelState.Remove("AssetNumber");
            ModelState.Remove("AssignedToUserId");
            ModelState.Remove("RepairAssignedToUserId");

            if (ModelState.IsValid)
            {
                try
                {
                    int count = db.Assets.Count() + 1;
                    asset.AssetNumber = "AST" + count.ToString("D4");

                    while (db.Assets.Any(a => a.AssetNumber == asset.AssetNumber))
                    {
                        count++;
                        asset.AssetNumber = "AST" + count.ToString("D4");
                    }

                    asset.DateCreated = DateTime.Now;
                    asset.IsActive = true;
                    asset.IsAssigned = false;
                    asset.Status = "Available";
                    asset.Condition = "New";
                    asset.RepairRequested = false;
                    asset.RepairStatus = null;
                    asset.LastUpdated = DateTime.Now;

                    if (string.IsNullOrEmpty(asset.Location) && !string.IsNullOrEmpty(asset.LocationAddress))
                    {
                        asset.Location = asset.LocationAddress;
                    }

                    db.Assets.Add(asset);
                    db.SaveChanges();

                    TempData["Success"] = $"Asset '{asset.AssetName}' created successfully with number: {asset.AssetNumber}";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating asset: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while creating the asset. Please try again.");
                }
            }

            ViewBag.Categories = new SelectList(Categories, asset.Category);
            ViewBag.Conditions = new SelectList(Conditions, asset.Condition);
            ViewBag.Statuses = new SelectList(Statuses, asset.Status);

            return View(asset);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = new SelectList(Categories, asset.Category);
            ViewBag.Conditions = new SelectList(Conditions, asset.Condition);
            ViewBag.Statuses = new SelectList(Statuses, asset.Status);
            ViewBag.Locations = new SelectList(Locations, asset.Location);

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult Edit(Asset asset)
        {
            if (ModelState.IsValid)
            {
                var existingAsset = db.Assets.Find(asset.AssetId);
                if (existingAsset == null)
                {
                    return HttpNotFound();
                }

                existingAsset.AssetName = asset.AssetName;
                existingAsset.Category = asset.Category;
                existingAsset.Description = asset.Description;
                existingAsset.Brand = asset.Brand;
                existingAsset.Model = asset.Model;
                existingAsset.SerialNumber = asset.SerialNumber;
                existingAsset.PurchaseDate = asset.PurchaseDate;
                existingAsset.PurchasePrice = asset.PurchasePrice;
                existingAsset.Supplier = asset.Supplier;
                existingAsset.Condition = asset.Condition;
                existingAsset.Location = asset.Location;
                existingAsset.WarrantyExpiry = asset.WarrantyExpiry;
                existingAsset.LastMaintenanceDate = asset.LastMaintenanceDate;
                existingAsset.NextMaintenanceDate = asset.NextMaintenanceDate;
                existingAsset.Notes = asset.Notes;
                existingAsset.LastUpdated = DateTime.Now;

                if (asset.Condition == "Poor" || asset.Condition == "Damaged" || asset.Condition == "For Repair")
                {
                    if (!existingAsset.RepairRequested)
                    {
                        existingAsset.RepairRequested = true;
                        existingAsset.RepairRequestDate = DateTime.Now;
                        existingAsset.RepairStatus = "Pending";
                        existingAsset.Status = "Pending Repair";

                        existingAsset.Notes = (existingAsset.Notes ?? "") + "\n" +
                            DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                            " - Repair automatically requested due to condition: " + asset.Condition;
                    }
                }
                else
                {
                    if (existingAsset.RepairRequested && existingAsset.RepairStatus != "In Progress" && existingAsset.RepairStatus != "Completed")
                    {
                        existingAsset.RepairRequested = false;
                        existingAsset.RepairStatus = null;
                        if (existingAsset.Status == "Pending Repair")
                        {
                            existingAsset.Status = "Available";
                        }
                    }
                }

                db.SaveChanges();

                TempData["Success"] = $"Asset '{existingAsset.AssetName}' updated successfully.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(Categories, asset.Category);
            ViewBag.Conditions = new SelectList(Conditions, asset.Condition);
            ViewBag.Statuses = new SelectList(Statuses, asset.Status);
            ViewBag.Locations = new SelectList(Locations, asset.Location);

            return View(asset);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult UpdateStatus(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            ViewBag.Conditions = new SelectList(Conditions, asset.Condition);
            ViewBag.Statuses = new SelectList(Statuses, asset.Status);

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult UpdateStatus(int id, string Condition, string Status, string Comment)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            asset.Condition = Condition;
            asset.Status = Status;
            asset.LastUpdated = DateTime.Now;

            if (!string.IsNullOrEmpty(Comment))
            {
                asset.Notes = (asset.Notes ?? "") + "\n" +
                    DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                    " - Status Update: " + Comment;
            }

            if (Condition == "Poor" || Condition == "Damaged" || Condition == "For Repair")
            {
                if (!asset.RepairRequested)
                {
                    asset.RepairRequested = true;
                    asset.RepairRequestDate = DateTime.Now;
                    asset.RepairStatus = "Pending";
                    asset.Status = "Pending Repair";

                    asset.Notes = (asset.Notes ?? "") + "\n" +
                        DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                        " - Repair automatically requested due to condition: " + Condition;
                }
            }
            else
            {
                if (asset.RepairRequested && asset.RepairStatus != "In Progress" && asset.RepairStatus != "Completed")
                {
                    asset.RepairRequested = false;
                    asset.RepairStatus = null;
                    if (asset.Status == "Pending Repair")
                    {
                        asset.Status = "Available";
                    }
                }
            }

            db.SaveChanges();

            TempData["Success"] = $"Asset '{asset.AssetName}' status updated successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult RequestRepair(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (asset.RepairRequested)
            {
                TempData["Error"] = "A repair request has already been submitted for this asset.";
                return RedirectToAction("Index");
            }

            if (asset.Status == "Pending Repair" || asset.Status == "In Repair")
            {
                TempData["Error"] = "This asset is already in the repair process.";
                return RedirectToAction("Index");
            }

            if (asset.Condition != "Poor" && asset.Condition != "Damaged" && asset.Condition != "For Repair")
            {
                TempData["Error"] = "This asset does not need repair. Current condition: " + asset.Condition;
                return RedirectToAction("Index");
            }

            var technicianUsers = GetTechnicians();
            ViewBag.TechnicianList = new SelectList(technicianUsers, "UserId", "FullName");

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult RequestRepair(int id, string RepairAssignedToUserId, string RepairReport)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(RepairAssignedToUserId))
            {
                ModelState.AddModelError("", "Please select a technician to assign the repair to.");
                ViewBag.TechnicianList = new SelectList(GetTechnicians(), "UserId", "FullName");
                return View(asset);
            }

            var technician = db.Staff.FirstOrDefault(s => s.UserId == RepairAssignedToUserId && s.IsActive);
            if (technician == null)
            {
                ModelState.AddModelError("", "Selected technician is not available.");
                ViewBag.TechnicianList = new SelectList(GetTechnicians(), "UserId", "FullName");
                return View(asset);
            }

            asset.RepairRequested = true;
            asset.RepairRequestDate = DateTime.Now;
            asset.RepairStatus = "Pending";
            asset.Status = "Pending Repair";
            asset.RepairAssignedToUserId = RepairAssignedToUserId; 
            asset.RepairReport = RepairReport;
            asset.RepairRequestedBy = User.Identity.GetUserId();
            asset.LastUpdated = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = $"Repair request submitted for '{asset.AssetName}'. Assigned to: {technician.FullName}. Awaiting Department Head approval.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult ApproveRepair(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                return HttpNotFound();
            }

            if (asset.RepairStatus != "Completed")
            {
                TempData["Error"] = "This repair has not been completed yet.";
                return RedirectToAction("Index");
            }

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult ApproveRepair(int id, string Notes)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                return HttpNotFound();
            }

            asset.RepairStatus = "Approved";
            asset.Status = "Available";
            asset.Condition = "Good";
            asset.RepairRequested = false;
            asset.IsAssigned = false;
            asset.AssignedToUserId = null;
            asset.DateAssigned = null;
            asset.LastUpdated = DateTime.Now;

            if (!string.IsNullOrEmpty(Notes))
            {
                asset.Notes = (asset.Notes ?? "") + "\n" + DateTime.Now.ToString("dd MMM yyyy HH:mm") + " - Approval Notes: " + Notes;
            }

            db.SaveChanges();

            TempData["Success"] = $"Repair for '{asset.AssetName}' has been approved. Asset is now available.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Assign(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (asset.Status == "Assigned")
            {
                TempData["Error"] = "This asset is already assigned.";
                return RedirectToAction("Index");
            }

            if (asset.Condition == "Poor" || asset.Condition == "Damaged" || asset.Condition == "For Repair")
            {
                TempData["Error"] = "This asset needs repair before it can be assigned. Please update the status first.";
                return RedirectToAction("Index");
            }

            if (asset.Status != "Available")
            {
                TempData["Error"] = "This asset is not available for assignment. Current status: " + asset.Status;
                return RedirectToAction("Index");
            }

            var staffList = db.Staff.Where(s => s.IsActive).ToList();
            ViewBag.StaffList = new SelectList(staffList, "UserId", "FullName");

            if (!string.IsNullOrEmpty(asset.AssignedToUserId))
            {
                var staff = db.Staff.FirstOrDefault(s => s.UserId == asset.AssignedToUserId);
                ViewBag.AssignedStaffName = staff?.FullName ?? "Unknown";
            }
            else
            {
                ViewBag.AssignedStaffName = null;
            }

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult Assign(int id, string AssignToUserId)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(AssignToUserId))
            {
                ModelState.AddModelError("", "Please select a staff member to assign this asset to.");
                var staffList = db.Staff.Where(s => s.IsActive).ToList();
                ViewBag.StaffList = new SelectList(staffList, "UserId", "FullName");
                return View(asset);
            }

            var staff = db.Staff.FirstOrDefault(s => s.UserId == AssignToUserId);
            if (staff == null)
            {
                ModelState.AddModelError("", "Selected staff member does not exist.");
                var staffList = db.Staff.Where(s => s.IsActive).ToList();
                ViewBag.StaffList = new SelectList(staffList, "UserId", "FullName");
                return View(asset);
            }

            asset.AssignedToUserId = AssignToUserId;
            asset.IsAssigned = true;
            asset.Status = "Assigned";
            asset.DateAssigned = DateTime.Now;
            asset.DateReturned = null;
            asset.LastUpdated = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = $"Asset '{asset.AssetName}' assigned to {staff.FullName} successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Unassign(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (asset.Status != "Assigned")
            {
                TempData["Error"] = "This asset is not currently assigned.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(asset.AssignedToUserId))
            {
                var staff = db.Staff.FirstOrDefault(s => s.UserId == asset.AssignedToUserId);
                ViewBag.StaffName = staff?.FullName ?? "Unknown";
            }
            else
            {
                ViewBag.StaffName = "Unknown";
            }

            return View(asset);
        }

        [HttpPost, ActionName("Unassign")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult UnassignConfirmed(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            var staffName = db.Staff.FirstOrDefault(s => s.UserId == asset.AssignedToUserId)?.FullName ?? "Unknown";

            asset.AssignedToUserId = null;
            asset.IsAssigned = false;
            asset.Status = "Available";
            asset.DateReturned = DateTime.Now;
            asset.LastUpdated = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = $"Asset '{asset.AssetName}' unassigned from {staffName} successfully.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Decommission(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (asset.Status == "Assigned")
            {
                TempData["Error"] = "This asset is currently assigned. Please unassign it first.";
                return RedirectToAction("Index");
            }

            if (asset.Status == "Disposed")
            {
                TempData["Error"] = "This asset has already been decommissioned.";
                return RedirectToAction("Index");
            }

            return View(asset);
        }

        [HttpPost, ActionName("Decommission")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult DecommissionConfirmed(int id, string DecommissionReason)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            asset.Status = "Disposed";
            asset.IsActive = false;
            asset.LastUpdated = DateTime.Now;

            if (!string.IsNullOrEmpty(DecommissionReason))
            {
                asset.Notes = (asset.Notes ?? "") + "\n" +
                    DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                    " - Asset Decommissioned: " + DecommissionReason;
            }

            db.SaveChanges();

            TempData["Success"] = $"Asset '{asset.AssetName}' has been decommissioned.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                return HttpNotFound();
            }

            if (!string.IsNullOrEmpty(asset.AssignedToUserId))
            {
                var staff = db.Staff.FirstOrDefault(s => s.UserId == asset.AssignedToUserId);
                ViewBag.AssignedStaffName = staff?.FullName ?? "Unknown";
            }

            if (!string.IsNullOrEmpty(asset.RepairAssignedToUserId))
            {
                var staff = db.Staff.FirstOrDefault(s => s.UserId == asset.RepairAssignedToUserId);
                ViewBag.RepairStaffName = staff?.FullName ?? "Unknown";
            }

            return View(asset);
        }

        [Authorize(Roles = "Admin, AssetManager")]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                return HttpNotFound();
            }
            return View(asset);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult DeleteConfirmed(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset != null)
            {
                db.Assets.Remove(asset);
                db.SaveChanges();
                TempData["Success"] = $"Asset '{asset.AssetName}' deleted successfully.";
            }
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Technician")]
        [HttpGet]
        public JsonResult GetRepairCount()
        {
            var userId = User.Identity.GetUserId();
            var count = db.Assets
                .Count(a => a.RepairAssignedToUserId == userId &&
                           a.RepairStatus == "Pending");

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

 
        private List<Staff> GetTechnicians()
        {
            try
            {
                var roleManager = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(db));
                var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(db));

                var technicianRole = roleManager.FindByName("Technician");
                var technicianUsers = new List<Staff>();

                if (technicianRole != null)
                {

                    var allUsers = userManager.Users.ToList();

                    var technicianUserIds = new List<string>();
                    foreach (var user in allUsers)
                    {
                        if (userManager.IsInRole(user.Id, "Technician"))
                        {
                            technicianUserIds.Add(user.Id);
                        }
                    }

                    technicianUsers = db.Staff
                        .Where(s => technicianUserIds.Contains(s.UserId) && s.IsActive)
                        .ToList();
                }

                return technicianUsers;
            }
            catch
            {
                return new List<Staff>();
            }
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
        public ActionResult AssignTechnician(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (asset.RepairStatus != "Pending" && asset.RepairStatus != "Approved")
            {
                TempData["Error"] = "This asset does not need a technician assignment. Current status: " + asset.RepairStatus;
                return RedirectToAction("Index");
            }

            var technicianUsers = GetTechnicians();
            ViewBag.TechnicianList = new SelectList(technicianUsers, "UserId", "FullName", asset.RepairAssignedToUserId);

            return View(asset);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult AssignTechnician(int id, string RepairAssignedToUserId)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(RepairAssignedToUserId))
            {
                ModelState.AddModelError("", "Please select a technician to assign.");
                var technicianUsers = GetTechnicians();
                ViewBag.TechnicianList = new SelectList(technicianUsers, "UserId", "FullName");
                return View(asset);
            }

            var technician = db.Staff.FirstOrDefault(s => s.UserId == RepairAssignedToUserId && s.IsActive);
            if (technician == null)
            {
                ModelState.AddModelError("", "Selected technician is not available.");
                var technicianUsers = GetTechnicians();
                ViewBag.TechnicianList = new SelectList(technicianUsers, "UserId", "FullName");
                return View(asset);
            }

            asset.RepairAssignedToUserId = RepairAssignedToUserId;
            asset.LastUpdated = DateTime.Now;

            if (asset.RepairStatus == null)
            {
                asset.RepairStatus = "Pending";
                asset.Status = "Pending Repair";
            }

            db.SaveChanges();

            TempData["Success"] = $"Technician '{technician.FullName}' has been assigned to repair '{asset.AssetName}'.";
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public ActionResult Disposal(int id)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }
            if (asset.Status == "Disposed")
            {
                TempData["Error"] = "This asset has already been disposed.";
                return RedirectToAction("Index");
            }

            if (asset.Status == "Repair")
            {
                TempData["Error"] = "This asset is currently in repair and cannot be disposed.";
                return RedirectToAction("Index");
            }

            ViewBag.DisposalMethods = new SelectList(new List<string>
    {
        "Auction",
        "Scrap",
        "Donation",
        "Recycle",
        "Trade-in",
        "Write-off"
    });

            return View(asset);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, AssetManager")]
        public ActionResult DisposalConfirmed(int id, string DisposalMethod, string DisposalNotes)
        {
            var asset = db.Assets.Find(id);
            if (asset == null)
            {
                TempData["Error"] = "Asset not found.";
                return RedirectToAction("Index");
            }

            asset.Status = "Disposed";
            asset.IsActive = false;
            asset.LastUpdated = DateTime.Now;

            asset.Notes = (asset.Notes ?? "") + "\n" +
                DateTime.Now.ToString("dd MMM yyyy HH:mm") +
                " - Asset Disposed: " +
                "Method: " + DisposalMethod +
                (string.IsNullOrEmpty(DisposalNotes) ? "" : " - Notes: " + DisposalNotes);

            db.SaveChanges();

            TempData["Success"] = $"Asset '{asset.AssetName}' has been disposed via {DisposalMethod}.";
            return RedirectToAction("Index");
        }

        private bool HasPendingDisposalRequest(int assetId)
        {
            return db.DisposalRequests
                .Any(r => r.AssetId == assetId &&
                          (r.Status == "Pending" || r.Status == "Reviewed" || r.Status == "Approved"));
        }

        [Authorize] 
        [HttpGet]
        public ActionResult MyAssets()
        {
            var userId = User.Identity.GetUserId();

            var myAssets = db.Assets
                .Where(a => a.AssignedToUserId == userId) 
                .ToList();

            return View(myAssets);
        }
    }
}