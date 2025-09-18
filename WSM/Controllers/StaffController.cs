using System.Linq;
using System.Text.RegularExpressions;
using EllipticCurve.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WMS.Models;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers
{
    public class StaffController : Controller
    {
        private readonly DB db;

        public StaffController(DB context)
        {
            db = context;

        }

        public IActionResult ReadStaff(string? id, string? sort, string? dir, int page = 1)
        {
            // For Layout.cshtml search bar
            ViewBag.SearchContext = "Staff";
            ViewBag.SearchPlaceholder = "Search by Staff ID /Name";

            // Searching
            ViewBag.Name = id = id?.Trim() ?? "";
            var searched = db.Staff.AsQueryable();

            if (!string.IsNullOrEmpty(id))
            {
                searched = searched.Where(s => s.Name.Contains(id) || s.Id.Contains(id));
            }

            //sorting
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            Func<Staff, object> fn = sort switch
            {
                "Id" => s => s.Id,
                "Name" => s => s.Name,
                "Email" => s => s.Email,
                "Phone No" => s => s.PhoneNo,
                "Salary" => s => s.Salary,
                _ => s => s.Id
            };

            var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);

            // Paging
            if (page < 1)
            {
                return RedirectToAction(null, new { id, sort, dir, page = 1 });
            }

            var m = sorted.ToPagedList(page, 10);

            if (page > m.PageCount && m.PageCount > 0)
            {
                return RedirectToAction(null, new { id, sort, dir, page = m.PageCount });
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ReadStaff", m);
            }



            ViewBag.Ingredients = db.Ingredients.ToList();
            return View(m);
            //int pageSize = 5;

        }



        //GET: Create Staff
        public IActionResult CreateStaff() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStaff(Staff model)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            var adminId = HttpContext.Session.GetString("StaffAdminId");
            if (string.IsNullOrEmpty(companyId) || string.IsNullOrEmpty(adminId))
                return RedirectToAction("Login", "Authorization");

            // Assign system-generated values
            model.Id = GenerateSequentialId();
            model.CompanyId = companyId;
            model.AdminId = adminId;

            // Clear validation errors for auto-generated fields
            ModelState.Remove("Id");
            ModelState.Remove("CompanyId");
            ModelState.Remove("AdminId");
            ModelState.Remove("Admin"); // navigation property, not from form

            // --- Validations ---
            if (!Regex.IsMatch(model.PhoneNo, @"^01[0-9]{8,13}$"))
                ModelState.AddModelError("PhoneNo", "Phone number must start with '01' and be 10–15 digits long.");

            if (!Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,20}$"))
                ModelState.AddModelError("Password", "Password must be 8–20 chars, include uppercase, lowercase, digit, and special char.");

            if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ModelState.AddModelError("Email", "Invalid email address.");

            if (db.Staff.Any(s => s.Email == model.Email && s.CompanyId == companyId))
                ModelState.AddModelError("Email", "The email is already exist");

            if (model.Salary <= 0)
                ModelState.AddModelError("Salary", "Salary must be greater than 0.");

            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<Staff>();
                model.Password = hasher.HashPassword(model, model.Password);

                db.Staff.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Staff '{model.Name}' created successfully.";
                return RedirectToAction("ReadStaff");
            }

            return View(model);
        }


        // GET: Edit Staff
        public IActionResult EditStaff(string id)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            var staff = db.Staff.FirstOrDefault(s => s.Id == id && s.CompanyId == companyId);

            if (staff == null) return NotFound();

            var model = new EditStaffViewModel
            {
                Id = staff.Id,
                Name = staff.Name,
                Email = staff.Email,
                PhoneNo = staff.PhoneNo,
                Salary = staff.Salary
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStaff(EditStaffViewModel model)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            if (string.IsNullOrEmpty(companyId))
                return RedirectToAction("Login", "Authorization");

            if (ModelState.IsValid)
            {
                var staff = db.Staff.FirstOrDefault(s => s.Id == model.Id && s.CompanyId == companyId);
                if (staff == null) return NotFound();

                // ✅ Only allow editing Salary
                staff.Salary = model.Salary;

                db.SaveChanges();
                TempData["SuccessMessage"] = $"Staff '{staff.Name}' salary updated successfully.";
                return RedirectToAction("ReadStaff");
            }

            return View(model);
        }



        // GET: Delete Staff
        public IActionResult DeleteStaff(string id)
        {
            var companyId = HttpContext.Session.GetString("CompanyId");
            var staff = db.Staff.FirstOrDefault(s => s.Id == id && s.CompanyId == companyId);

            if (staff == null) return NotFound();

            db.Staff.Remove(staff);
            db.SaveChanges();

            TempData["SuccessMessage"] = $"Staff '{staff.Name}' deleted successfully.";
            return RedirectToAction("ReadStaff");
        }

        // Helper to generate sequential IDs like S0001
        private string GenerateSequentialId()
        {
            var staffIds = db.Staff
                .Where(s => s.Id.StartsWith("S") && s.Id.Length == 5)
                .Select(s => s.Id)
                .ToList();

            int maxNumber = staffIds
                .Select(id => int.TryParse(id.Substring(1), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            int nextNumber = maxNumber + 1;
            if (nextNumber > 9999)
                throw new InvalidOperationException("Maximum staff IDs reached (S9999).");

            return $"S{nextNumber:D4}";
        }

        // GET: Staff UpdateProfile
        public IActionResult UpdateProfile()
        {
            var staffId = HttpContext.Session.GetString("StaffAdminId"); // 
            if (string.IsNullOrEmpty(staffId))
                return RedirectToAction("Login", "Authorization");

            var staff = db.Staff.FirstOrDefault(s => s.Id == staffId);
            if (staff == null) return NotFound();

            var vm = new StaffUpdateProfileViewModel
            {
                Id = staff.Id,
                Name = staff.Name,
                Email = staff.Email,
                PhoneNo = staff.PhoneNo,
                PhotoPath = staff.PhotoPath
            };

            return View(vm); // 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(StaffUpdateProfileViewModel model, IFormFile? Photo)
        {
            var staffId = HttpContext.Session.GetString("StaffAdminId");
            if (string.IsNullOrEmpty(staffId))
                return RedirectToAction("Login", "Authorization");

            var staff = db.Staff.FirstOrDefault(s => s.Id == staffId);
            if (staff == null) return NotFound();

            if (ModelState.IsValid)
            {
                staff.Name = model.Name;
                staff.Email = model.Email;
                staff.PhoneNo = model.PhoneNo;

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

                    staff.PhotoPath = $"/images/profile/{fileName}";
                    HttpContext.Session.SetString("ProfilePhoto", staff.PhotoPath);
                }

                // Password change
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                    {
                        ModelState.AddModelError("CurrentPassword", "Please enter your current password to change it.");
                        return View(model);
                    }

                    var hasher = new PasswordHasher<Staff>();
                    var result = hasher.VerifyHashedPassword(staff, staff.Password, model.CurrentPassword);

                    if (result == PasswordVerificationResult.Failed)
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                        return View(model);
                    }

                    staff.Password = hasher.HashPassword(staff, model.NewPassword);
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction("UpdateProfile");
            }

            return View(model);
        }

    }
}
