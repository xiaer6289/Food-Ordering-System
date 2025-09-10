using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WSM.Models;

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
        public IActionResult Admins(string searchString, string sortOrder)
        {
            searchString = searchString?.Trim();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IdSortParm"] = sortOrder == "Id" ? "id_desc" : "Id";
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
            ViewData["PhoneSortParm"] = sortOrder == "Phone" ? "phone_desc" : "Phone";

            var companyId = HttpContext.Session.GetString("CompanyId");

            var admins = db.Admins.Where(a => a.CompanyId == companyId).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                admins = admins.Where(a =>
                    a.Name.Contains(searchString) ||
                    a.PhoneNo.Contains(searchString) ||
                    a.Email.Contains(searchString));
            }

            admins = sortOrder switch
            {
                "id_desc" => admins.OrderByDescending(a => a.Id),
                "Id" => admins.OrderBy(a => a.Id),
                "name_desc" => admins.OrderByDescending(a => a.Name),
                "Email" => admins.OrderBy(a => a.Email),
                "email_desc" => admins.OrderByDescending(a => a.Email),
                "Phone" => admins.OrderBy(a => a.PhoneNo),
                "phone_desc" => admins.OrderByDescending(a => a.PhoneNo),
                _ => admins.OrderBy(a => a.Name),
            };

            return View(admins.ToList());
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

            if (!string.IsNullOrEmpty(model.PhoneNo) && !Regex.IsMatch(model.PhoneNo, @"^01[0-9]{8,13}$"))
            {
                ModelState.AddModelError("PhoneNo", "Phone number must start with '01' and be 10 to 15 digits long.");
            }

            if (!string.IsNullOrEmpty(model.Password) && !Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,20}$"))
            {
                ModelState.AddModelError("Password", "Password must be 8 to 20 characters long, with at least one uppercase, one lowercase, one digit, and one special character (!@#$%^&*).");
            }

            if (!string.IsNullOrEmpty(model.Email) && !Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address.");
            }

            if (ModelState.IsValid)
            {
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

                // Update profile fields without needing a password
                admin.Name = model.Name;
                admin.Email = model.Email;
                admin.PhoneNo = model.PhoneNo;

                // Only hash & update password if a new one was provided
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    var hasher = new PasswordHasher<Admin>();
                    admin.Password = hasher.HashPassword(admin, model.NewPassword);
                }

                db.SaveChanges();
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

            if (admin.Staffs.Any())
            {
                ModelState.AddModelError("", "Cannot delete admin because they are linked to staff records.");
                return View("Admins", db.Admins.Where(a => a.CompanyId == companyId).ToList());
            }

            db.Admins.Remove(admin);
            db.SaveChanges();
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
