using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WSM.Models;

namespace WSM.Controllers
{
    public class HomeController : Controller
    {
        private readonly DB db;
        private readonly IPasswordHasher<string> _passwordHasher;

        public HomeController(DB db, IPasswordHasher<string> passwordHasher)
        {
            this.db = db;
            _passwordHasher = passwordHasher;
        }

        // Home page after company login
        public IActionResult Both()
        {
            string? companyId = HttpContext.Session.GetString("CompanyId");
            if (companyId == null)
                return RedirectToAction("Login", "Authorization");

            var company = db.Companies.Find(companyId);
            if (company == null)
                return RedirectToAction("Login", "Authorization");

            // Pass company info to view
            ViewBag.CompanyEmail = company.Email;
            ViewBag.CompanyLogo = company.LogoPath;
            ViewBag.CompanyName = company.CompanyName;
            ViewBag.CompanyDescription = company.Description;

            string? staffId = HttpContext.Session.GetString("StaffAdminId");
            if (!string.IsNullOrEmpty(staffId))
            {
                var staff = db.Staff.FirstOrDefault(s => s.Id == staffId && s.CompanyId == companyId);
                if (staff != null && !string.IsNullOrEmpty(staff.PhotoPath))
                {
                    HttpContext.Session.SetString("ProfilePhoto", staff.PhotoPath);
                }
            }


            return View();
        }

        // POST: Login with Email or ID + Password
        [HttpPost]
        public IActionResult Login(string loginInput, string Password)
        {
            string? companyId = HttpContext.Session.GetString("CompanyId");
            if (string.IsNullOrEmpty(companyId))
                return RedirectToAction("Login", "Authorization");

            // Check Admin
            var admin = db.Admins.FirstOrDefault(a =>
                a.CompanyId == companyId &&
                (a.Email == loginInput || a.Id == loginInput)
            );

            if (admin != null)
            {
                // **Use the same key as when hashed**
                var key = admin.Email; // MUST match what was used in HashPassword
                var result = _passwordHasher.VerifyHashedPassword(key, admin.Password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("StaffAdminId", admin.Id);
                    HttpContext.Session.SetString("Role", "Admin");
                    if (!string.IsNullOrEmpty(admin.PhotoPath))
                        HttpContext.Session.SetString("ProfilePhoto", admin.PhotoPath);

                    return RedirectToAction("Index", "Table"); // Redirect to Table page
                }
            }

            // Check Staff
            var staff = db.Staff.FirstOrDefault(s =>
                s.CompanyId == companyId &&
                (s.Email == loginInput || s.Id == loginInput)
            );

            if (staff != null)
            {
                var key = staff.Email; // MUST match what was used when hashing
                var result = _passwordHasher.VerifyHashedPassword(key, staff.Password, Password);

                if (result == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("StaffAdminId", staff.Id);
                    HttpContext.Session.SetString("Role", "Staff");
                    if (!string.IsNullOrEmpty(staff.PhotoPath))
                        HttpContext.Session.SetString("ProfilePhoto", staff.PhotoPath);

                    return RedirectToAction("Index", "Table"); // Redirect to Table page
                }
            }

            TempData["ErrorMessage"] = "Invalid ID/email or password";
            return RedirectToAction("Both");
        }


        // Admin page
        public IActionResult Admin()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Both");
            return View();
        }

        // Staff page
        public IActionResult Staff()
        {
            if (HttpContext.Session.GetString("Role") != "Staff")
                return RedirectToAction("Both");
            return View();
        }
    }
}
