using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Municipality_System_Administration.Models;

namespace Municipality_System_Administration.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();




        [HttpGet]
        public ActionResult Index()
        {
            var staff = db.Staff.ToList();
            return View(staff);
        }

        //==============================
        // MY PROFILE - GET
        //==============================
        [Authorize]
        [HttpGet]
        public ActionResult MyProfile()
        {
            var userId = User.Identity.GetUserId();
            var staff = db.Staff.FirstOrDefault(s => s.UserId == userId);

            if (staff == null)
            {
                // If staff record doesn't exist, create a basic one
                var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(db));
                var user = userManager.FindById(userId);

                if (user != null)
                {
                    staff = new Staff
                    {
                        UserId = userId,
                        FirstName = user.UserName,
                        LastName = "",
                        Email = user.Email,
                        PhoneNumber = "",
                        EmployeeNumber = "EMP" + (db.Staff.Count() + 1).ToString("D3"),
                        IsActive = true,
                        DateCreated = DateTime.Now
                    };
                    db.Staff.Add(staff);
                    db.SaveChanges();
                }
                else
                {
                    return HttpNotFound();
                }
            }

            // Get user roles
            var userManager2 = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(db));
            var roles = userManager2.GetRoles(userId);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "No Role";
            ViewBag.AllRoles = roles;

            return View(staff);
        }

        //==============================
        // MY PROFILE - POST (Update Profile)
        //==============================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MyProfile(Staff staff)
        {
            if (ModelState.IsValid)
            {
                var existingStaff = db.Staff.Find(staff.StaffId);
                if (existingStaff == null)
                {
                    return HttpNotFound();
                }

                existingStaff.FirstName = staff.FirstName;
                existingStaff.LastName = staff.LastName;
                existingStaff.PhoneNumber = staff.PhoneNumber;
                existingStaff.Email = staff.Email;


                db.SaveChanges();

                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("MyProfile");
            }

            // Get user roles
            var userId = User.Identity.GetUserId();
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(db));
            var roles = userManager.GetRoles(userId);
            ViewBag.UserRole = roles.FirstOrDefault() ?? "No Role";
            ViewBag.AllRoles = roles;

            return View(staff);
        }




        [HttpGet]
        public ActionResult Create()
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(db));

            ViewBag.Role = new SelectList(roleManager.Roles, "Name", "Name");

            return View(new Staff());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Staff staff, string Role, string password)
        {
            ModelState.Remove("EmployeeNumber");

            if (ModelState.IsValid)
            {
                int count = db.Staff.Count();
                int number = count + 1;

                staff.EmployeeNumber = "EMP" + number.ToString("D3");

                while (db.Staff.Any(x => x.EmployeeNumber == staff.EmployeeNumber))
                {
                    number++;
                    staff.EmployeeNumber = "EMP" + number.ToString("D3");
                }

                staff.DateCreated = DateTime.Now;
                staff.IsActive = true;

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);

                var user = new ApplicationUser
                {
                    UserName = staff.Email,
                    Email = staff.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    DateCreated = DateTime.Now
                };

                var result = userManager.Create(user, password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(Role))
                    {
                        var roleResult = userManager.AddToRole(user.Id, Role);

                        if (!roleResult.Succeeded)
                        {
                            userManager.Delete(user);

                            foreach (var error in roleResult.Errors)
                            {
                                ModelState.AddModelError("", "Role Error: " + error);
                            }

                            var roleManager = new RoleManager<IdentityRole>(
                                new RoleStore<IdentityRole>(db));
                            ViewBag.Role = new SelectList(roleManager.Roles, "Name", "Name", Role);

                            return View(staff);
                        }
                    }
                    else
                    {
                        userManager.Delete(user);
                        ModelState.AddModelError("", "Please select a role for the staff member.");

                        var roleManager = new RoleManager<IdentityRole>(
                            new RoleStore<IdentityRole>(db));
                        ViewBag.Role = new SelectList(roleManager.Roles, "Name", "Name", Role);

                        return View(staff);
                    }

                    staff.UserId = user.Id;

                    db.Staff.Add(staff);
                    db.SaveChanges();

                    TempData["Success"] = $"Staff member '{staff.FullName}' created successfully with role: {Role}";
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", "User Creation Error: " + error);
                    }

                    var roleManager = new RoleManager<IdentityRole>(
                        new RoleStore<IdentityRole>(db));
                    ViewBag.Role = new SelectList(roleManager.Roles, "Name", "Name", Role);

                    return View(staff);
                }
            }

            var roleManager2 = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(db));
            ViewBag.Role = new SelectList(roleManager2.Roles, "Name", "Name", Role);

            return View(staff);
        }





        [HttpGet]
        public ActionResult Edit(int id)
        {
            var staff = db.Staff.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(db));
            var roles = userManager.GetRoles(staff.UserId);
            string currentRole = roles.FirstOrDefault() ?? "";

            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(db));
            ViewBag.Role = new SelectList(roleManager.Roles, "Name", "Name", currentRole);

            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Staff staff, string Role)
        {
            if (ModelState.IsValid)
            {
                var existingStaff = db.Staff.Find(staff.StaffId);
                if (existingStaff == null)
                {
                    return HttpNotFound();
                }

                existingStaff.FirstName = staff.FirstName;
                existingStaff.LastName = staff.LastName;
                existingStaff.PhoneNumber = staff.PhoneNumber;
                existingStaff.Email = staff.Email;

                var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(db));

                var currentRoles = userManager.GetRoles(existingStaff.UserId);

                foreach (var role in currentRoles)
                {
                    userManager.RemoveFromRole(existingStaff.UserId, role);
                }

                if (!string.IsNullOrEmpty(Role))
                {
                    userManager.AddToRole(existingStaff.UserId, Role);
                }

                db.SaveChanges();
                TempData["Success"] = $"Staff member '{existingStaff.FullName}' updated successfully.";
                return RedirectToAction("Index");
            }

            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(db));
            ViewBag.Role = new SelectList(roleManager.Roles, "Name", "Name", Role);

            return View(staff);
        }


        public ActionResult Disable(int id)
        {

            var staff =
            db.Staff.Find(id);



            staff.IsActive = false;


            var user =
            db.Users.Find(staff.UserId);


            user.IsActive = false;


            db.SaveChanges();



            return RedirectToAction("Index");

        }


        public ActionResult Enable(int id)
        {


            var staff =
            db.Staff.Find(id);


            staff.IsActive = true;


            var user =
            db.Users.Find(staff.UserId);


            user.IsActive = true;


            db.SaveChanges();



            return RedirectToAction("Index");


        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            var staff = db.Staff.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var staff = db.Staff.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var staff = db.Staff.Find(id);
            if (staff != null)
            {
                var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(db));
                var user = userManager.FindById(staff.UserId);
                if (user != null)
                {
                    userManager.Delete(user);
                }

                db.Staff.Remove(staff);
                db.SaveChanges();

                TempData["Success"] = $"Staff member '{staff.FullName}' deleted successfully.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ResetPassword(int id)
        {
            var staff = db.Staff.Find(id);

            if (staff == null)
                return HttpNotFound();

            return View(staff);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id, string newPassword)
        {
            var staff = db.Staff.Find(id);

            if (staff == null)
                return HttpNotFound();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("", "Please enter a new password.");
                return View(staff);
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Password must contain at least 6 characters.");
                return View(staff);
            }

            if (string.IsNullOrEmpty(staff.UserId))
            {
                ModelState.AddModelError("", "This staff member does not have a system account.");
                return View(staff);
            }

            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var user = userManager.FindById(staff.UserId);

            if (user == null)
            {
                ModelState.AddModelError("", "User account could not be found.");
                return View(staff);
            }


            var passwordHasher = new PasswordHasher();

            user.PasswordHash = passwordHasher.HashPassword(newPassword);

            user.IsActive = true;

            var result = userManager.Update(user);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Password reset successfully for " + staff.FullName + ".";

                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            return View(staff);
        }





    }
}
