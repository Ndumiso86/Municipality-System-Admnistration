using Municipality_System_Administration.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Municipality_System_Administration.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //==============================
        // List Departments
        //==============================
        public ActionResult Index(string search)
        {
            var departments = db.Departments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                departments = departments.Where(x =>
                    x.DepartmentName.Contains(search));
            }

            return View(departments.OrderBy(x => x.DepartmentName).ToList());
        }

        //==============================
        // Create
        //==============================
        public ActionResult Create()
        {
            var model = new Department();

            model.IsActive = true;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                department.IsActive = true;

                db.Departments.Add(department);

                db.SaveChanges();

                TempData["Success"] = "Department created successfully.";

                return RedirectToAction("Index");
            }

            return View(department);
        }

        //==============================
        // Details
        //==============================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Department department = db.Departments.Find(id);

            if (department == null)
                return HttpNotFound();

            return View(department);
        }

        //==============================
        // Edit
        //==============================
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Department department = db.Departments.Find(id);

            if (department == null)
                return HttpNotFound();

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                db.Entry(department).State = EntityState.Modified;

                db.SaveChanges();

                TempData["Success"] = "Department updated successfully.";

                return RedirectToAction("Index");
            }

            return View(department);
        }

        //==============================
        // Disable
        //==============================
        public ActionResult Disable(int id)
        {
            var department = db.Departments.Find(id);

            if (department != null)
            {
                department.IsActive = false;

                db.SaveChanges();

                TempData["Success"] = "Department disabled.";
            }

            return RedirectToAction("Index");
        }

        //==============================
        // Enable
        //==============================
        public ActionResult Enable(int id)
        {
            var department = db.Departments.Find(id);

            if (department != null)
            {
                department.IsActive = true;

                db.SaveChanges();

                TempData["Success"] = "Department enabled.";
            }

            return RedirectToAction("Index");
        }

        //==============================
        // Delete
        //==============================
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Department department = db.Departments.Find(id);

            if (department == null)
                return HttpNotFound();

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Department department = db.Departments
                .Include(x => x.StaffMembers)
                .FirstOrDefault(x => x.DepartmentId == id);

            if (department.StaffMembers.Any())
            {
                TempData["Error"] = "Cannot delete a department that has employees.";

                return RedirectToAction("Index");
            }

            db.Departments.Remove(department);

            db.SaveChanges();

            TempData["Success"] = "Department deleted.";

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}