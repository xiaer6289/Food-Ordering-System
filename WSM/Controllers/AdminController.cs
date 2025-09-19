using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly DB db;

        public AdminController(DB db)
        {
            this.db = db;
        }

        // GET: /Admin/Admins
        [Authorize(Roles = "Admin")]
        public IActionResult Admins(string? id, string? sort, string? dir, int page = 1)
        {
            ViewBag.SearchContext = "Admin";
            ViewBag.SearchPlaceholder = "Search by Admin ID or Name";

            ViewBag.Name = id = id?.Trim() ?? "";

            var companyId = HttpContext.Session.GetString("CompanyId");
            if (string.IsNullOrEmpty(companyId))
            {
                return RedirectToAction("Login", "Authorization");
            }

            var admins = db.Admins
                           .Where(a => a.CompanyId == companyId)
                           .AsQueryable();

            if (!string.IsNullOrEmpty(id))
            {
                if (admins.Any(a => a.Id == id))
                {
                    admins = admins.Where(a => a.Id == id);
                }
                else
                {
                    admins = admins.Where(a => a.Name.Contains(id));
                }
            }

            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            Func<Admin, object> fn = sort switch
            {
                "Id" => a => a.Id,
                "Name" => a => a.Name,
                "Email" => a => a.Email,
                _ => a => a.Id
            };

            var sorted = dir == "des"
                ? admins.OrderByDescending(fn)
                : admins.OrderBy(fn);

            if (page < 1)
            {
                return RedirectToAction(null, new { id, sort, dir, page = 1 });
            }

            var pagedAdmins = sorted.ToPagedList(page, 5);

            if (page > pagedAdmins.PageCount && pagedAdmins.PageCount > 0)
            {
                return RedirectToAction(null, new { id, sort, dir, page = pagedAdmins.PageCount });
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Admins", pagedAdmins);
            }

            return View(pagedAdmins);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateAdmin(Admin model)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");

            model.Id = GenerateSequentialId();
            model.CompanyId = companyId;

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

        [Authorize(Roles = "Admin")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteAdmin(string id)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            var admin = db.Admins
                          .Include(a => a.Staffs)
                          .FirstOrDefault(a => a.Id == id && a.CompanyId == companyId);

            if (admin == null) return NotFound();

            var superAdminId = db.Admins
                                 .Where(a => a.CompanyId == companyId)
                                 .OrderBy(a => a.Id)
                                 .Select(a => a.Id)
                                 .FirstOrDefault();

            if (admin.Id == superAdminId)
            {
                TempData["ErrorMessage"] = $"Cannot delete the SuperAdmin '{admin.Name}'.";
                return RedirectToAction("Admins");
            }

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

        [Authorize(Roles = "Admin")]
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

                var superAdminId = db.Admins
                                     .Where(a => a.CompanyId == companyId)
                                     .OrderBy(a => a.Id)
                                     .Select(a => a.Id)
                                     .FirstOrDefault();

                var linkedAdmins = admins.Where(a => a.Staffs.Any()).ToList();
                var superAdminSelected = admins.Any(a => a.Id == superAdminId);

                if (superAdminSelected || linkedAdmins.Any())
                {
                    var errorMessages = new List<string>();
                    if (superAdminSelected)
                        errorMessages.Add("Cannot delete the SuperAdmin.");
                    if (linkedAdmins.Any())
                        errorMessages.Add($"Cannot delete these admins because they are linked to staff records: {string.Join(", ", linkedAdmins.Select(a => a.Name))}");

                    TempData["ErrorMessage"] = string.Join(" ", errorMessages);
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
                    HttpContext.Session.SetString("ProfilePhoto", admin.PhotoPath);
                }

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
