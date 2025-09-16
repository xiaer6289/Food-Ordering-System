using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers
{
    public class AdminController : Controller
    {
        private readonly DB db;

        public AdminController(DB db)
        {
            this.db = db;
        }

        // GET: /Admin/Admins
        public IActionResult Admins(string? id, string? sort, string? dir, int page = 1)
        {
            // Setup for search bar in Layout
            ViewBag.SearchContext = "Admin";
            ViewBag.SearchPlaceholder = "Search by Admin ID or Name";

            // Trim input
            ViewBag.Name = id = id?.Trim() ?? "";

            var companyId = HttpContext.Session.GetString("CompanyId");
            if (string.IsNullOrEmpty(companyId))
            {
                return RedirectToAction("Login", "Authorization");
            }

            // Base query
            var admins = db.Admins
                           .Where(a => a.CompanyId == companyId)
                           .AsQueryable();

            // Searching
            if (!string.IsNullOrEmpty(id))
            {
                // If input matches an exact Admin ID
                if (admins.Any(a => a.Id == id))
                {
                    admins = admins.Where(a => a.Id == id);
                }
                else
                {
                    // Otherwise, search by partial Name
                    admins = admins.Where(a => a.Name.Contains(id));
                }
            }

            // Sorting
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            Func<Admin, object> fn = sort switch
            {
                "Id" => a => a.Id,
                "Name" => a => a.Name,
                "Email" => a => a.Email,
                _ => a => a.Id // default sort by Id
            };

            var sorted = dir == "des"
                ? admins.OrderByDescending(fn)
                : admins.OrderBy(fn);

            // Paging
            if (page < 1)
            {
                return RedirectToAction(null, new { id, sort, dir, page = 1 });
            }

            var pagedAdmins = sorted.ToPagedList(page, 10);

            if (page > pagedAdmins.PageCount && pagedAdmins.PageCount > 0)
            {
                return RedirectToAction(null, new { id, sort, dir, page = pagedAdmins.PageCount });
            }

            // Support for AJAX partial rendering
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_AdminsPartial", pagedAdmins);
            }

            return View(pagedAdmins);
        }



        // GET: /Admin/CreateAdmin
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // POST: /Admin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAdmin(Admin model)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            if (string.IsNullOrEmpty(companyId))
            {
                return RedirectToAction("Login", "Authorization");
            }

            model.Id = GenerateSequentialId();
            model.CompanyId = companyId;

            // Clear validation errors for auto-generated fields
            if (ModelState.ContainsKey("Id"))
            {
                ModelState["Id"].Errors.Clear();
                ModelState["Id"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            if (ModelState.ContainsKey("CompanyId"))
            {
                ModelState["CompanyId"].Errors.Clear();
                ModelState["CompanyId"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }

            // Phone number validation
            if (!string.IsNullOrEmpty(model.PhoneNo) && !Regex.IsMatch(model.PhoneNo, @"^01[0-9]{8,13}$"))
            {
                ModelState.AddModelError("PhoneNo", "Phone number must start with '01' and be 10 to 15 digits long.");
            }

            // Password validation
            if (!string.IsNullOrEmpty(model.Password) &&
                !Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,20}$"))
            {
                ModelState.AddModelError("Password", "Password must be 8 to 20 characters long, with at least one uppercase, one lowercase, one digit, and one special character (!@#$%^&*).");
            }

            // Email validation
            if (!string.IsNullOrEmpty(model.Email) && !Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Hash password before saving
                    var hasher = new PasswordHasher<Admin>();
                    model.Password = hasher.HashPassword(model, model.Password);

                    db.Admins.Add(model);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"Admin '{model.Name}' added successfully.";
                    return RedirectToAction("Admins");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Failed to create admin: {ex.InnerException?.Message ?? ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: /Admin/EditAdmin/{id}
        public IActionResult EditAdmin(string id)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            var admin = db.Admins.FirstOrDefault(a => a.Id == id && a.CompanyId == companyId);

            if (admin == null) return NotFound();

            var model = new EditAdminViewModel
            {
                Id = admin.Id,
                Name = admin.Name,
                Email = admin.Email,
                PhoneNo = admin.PhoneNo
            };

            return View(model);
        }

        // POST: /Admin/EditAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAdmin(EditAdminViewModel model)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            if (string.IsNullOrEmpty(companyId))
            {
                return RedirectToAction("Login", "Authorization");
            }

            if (ModelState.IsValid)
            {
                var admin = db.Admins.FirstOrDefault(a => a.Id == model.Id && a.CompanyId == companyId);
                if (admin == null) return NotFound();

                admin.Name = model.Name;
                admin.Email = model.Email;
                admin.PhoneNo = model.PhoneNo;

                // If password was updated
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    var hasher = new PasswordHasher<Admin>();
                    admin.Password = hasher.HashPassword(admin, model.NewPassword);
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = $"Admin '{admin.Name}' updated successfully.";
                return RedirectToAction("Admins");
            }

            return View(model);
        }

        // GET: /Admin/DeleteAdmin/{id}
        public IActionResult DeleteAdmin(string id)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            var admin = db.Admins
                          .Include(a => a.Staffs)
                          .FirstOrDefault(a => a.Id == id && a.CompanyId == companyId);

            if (admin == null) return NotFound();

            // Prevent deletion if linked to staff
            if (admin.Staffs.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete admin '{admin.Name}' because they are linked to staff records.";
                return RedirectToAction("Admins");
            }

            db.Admins.Remove(admin);
            db.SaveChanges();

            TempData["SuccessMessage"] = $"Admin '{admin.Name}' deleted successfully.";
            return RedirectToAction("Admins");
        }

        // Helper to generate sequential admin IDs like A0001, A0002
        private string GenerateSequentialId()
        {
            var adminIds = db.Admins
                .Where(a => a.Id.StartsWith("A") && a.Id.Length == 5)
                .Select(a => a.Id)
                .ToList();

            int maxNumber = 0;
            foreach (var id in adminIds)
            {
                if (int.TryParse(id.Substring(1), out int number))
                {
                    if (number > maxNumber) maxNumber = number;
                }
            }

            int nextNumber = maxNumber + 1;
            if (nextNumber > 9999)
            {
                throw new InvalidOperationException("Maximum number of admin IDs reached (A9999).");
            }

            return $"A{nextNumber:D4}";
        }
    }
}
