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

            // Base query - only admins for this company
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

            var pagedAdmins = sorted.ToPagedList(page, 5);

            if (page > pagedAdmins.PageCount && pagedAdmins.PageCount > 0)
            {
                return RedirectToAction(null, new { id, sort, dir, page = pagedAdmins.PageCount });
            }

            // AJAX support for partial reload
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Admins", pagedAdmins);
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

            // Clear validation for auto-generated fields
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

            if (db.Admins.Any(a => a.Email == model.Email && a.CompanyId == companyId))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
            }

            //Validate password
            if (!string.IsNullOrEmpty(model.Password) &&
                !Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,20}$"))
            {
                ModelState.AddModelError("Password", "Password must be 8-20 characters with uppercase, lowercase, number, and special character.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                //Hash password
                var hasher = new PasswordHasher<Admin>();
                model.Password = hasher.HashPassword(model, model.Password);

                db.Admins.Add(model);
                db.SaveChanges();

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

            return View(model);
        }

        

        // GET: /Admin/EditAdmin
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

                db.SaveChanges();

                TempData["SuccessMessage"] = $"Admin '{admin.Name}' updated successfully.";
                return RedirectToAction("Admins");
            }

            return View(model);
        }

        // GET: /Admin/DeleteAdmin
        public IActionResult DeleteAdmin(string id)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            var admin = db.Admins
                          .Include(a => a.Staffs)
                          .FirstOrDefault(a => a.Id == id && a.CompanyId == companyId);

            if (admin == null) return NotFound();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMany(string[] ids)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");

            if (ids == null || ids.Length == 0)
            {
                TempData["ErrorMessage"] = "No admins selected for deletion.";
                return RedirectToAction("Admins");
            }

            try
            {
                var admins = db.Admins
                               .Include(a => a.Staffs)
                               .Where(a => ids.Contains(a.Id) && a.CompanyId == companyId)
                               .ToList();

                if (!admins.Any())
                {
                    TempData["ErrorMessage"] = "No matching admins found for deletion.";
                    return RedirectToAction("Admins");
                }

                var linkedAdmins = admins.Where(a => a.Staffs.Any()).ToList();
                if (linkedAdmins.Any())
                {
                    var names = string.Join(", ", linkedAdmins.Select(a => a.Name));
                    TempData["ErrorMessage"] = $"Cannot delete these admins because they are linked to staff records: {names}";
                    return RedirectToAction("Admins");
                }

                db.Admins.RemoveRange(admins);
                int deletedCount = db.SaveChanges();

                TempData["SuccessMessage"] = $"{deletedCount} admin(s) deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting admins: {ex.Message}";
            }

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

        [HttpGet]
        public IActionResult UpdateProfile()
        {
            var userId = HttpContext.Session.GetString("StaffAdminId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Authorization");

            var admin = db.Admins.FirstOrDefault(a => a.Id == userId);
            if (admin == null) return NotFound();

            var vm = new UpdateProfileViewModel
            {
                Id = admin.Id,
                Name = admin.Name,
                Email = admin.Email,
                PhoneNo = admin.PhoneNo,
                PhotoPath = admin.PhotoPath
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model, IFormFile? Photo)
        {
            var userId = HttpContext.Session.GetString("StaffAdminId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Authorization");

            var admin = db.Admins.FirstOrDefault(a => a.Id == userId);
            if (admin == null) return NotFound();

            if (ModelState.IsValid)
            {
                admin.Name = model.Name;
                admin.Email = model.Email;
                admin.PhoneNo = model.PhoneNo;

                // Handle photo upload
                if (Photo != null && Photo.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profile");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}_{Photo.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Photo.CopyToAsync(stream);
                    }

                    admin.PhotoPath = $"/images/profile/{fileName}";
                    HttpContext.Session.SetString("ProfilePhoto", admin.PhotoPath); // update session
                }

                // Optional password update
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                    {
                        ModelState.AddModelError("CurrentPassword", "Please enter your current password to change it.");
                        return View(model);
                    }

                    var hasher = new PasswordHasher<Admin>();
                    var result = hasher.VerifyHashedPassword(admin, admin.Password, model.CurrentPassword);

                    if (result == PasswordVerificationResult.Failed)
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                        return View(model);
                    }

                    admin.Password = hasher.HashPassword(admin, model.NewPassword);
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("UpdateProfile");
            }

            return View(model);
        }
    }
}
