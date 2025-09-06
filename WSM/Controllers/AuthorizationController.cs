using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WSM.Models;

public class AuthorizationController : Controller
{
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly DB _db;
    private readonly IWebHostEnvironment _env;

    public AuthorizationController(DB db, IPasswordHasher<string> passwordHasher, IWebHostEnvironment env)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _env = env;
    }

    // GET: /Authorization/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Authorization/Login
    [HttpPost]
    public IActionResult Login(string Email, string Password)
    {
        var company = _db.Companies.FirstOrDefault(c => c.Email == Email);

        if (company == null)
        {
            TempData["ErrorMessage"] = "This email is not registered.";
            return RedirectToAction("Login");
        }

        var result = _passwordHasher.VerifyHashedPassword(Email, company.PasswordHash, Password);
        if (result == PasswordVerificationResult.Success)
        {
            // Set session for logged-in company
            HttpContext.Session.SetInt32("CompanyId", company.Id);

            // Redirect based on first login
            if (company.IsFirstLogin)
                return RedirectToAction("FillCompanyProfile"); // first login go to fill profile
            else
                return RedirectToAction("Both", "Home"); // normal login go to  dashboard/home
        }
        else
        {
            TempData["ErrorMessage"] = "Incorrect password. Please try again.";
            return RedirectToAction("Login");
        }
    }


    // GET: /Authorization/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Authorization/Register
    [HttpPost]
    public IActionResult Register(string CompanyName, string Email, string Password, string ConfirmPassword, IFormFile Logo)
    {
        // 1. Validate passwords
        if (Password != ConfirmPassword)
        {
            TempData["ErrorMessage"] = "Passwords do not match.";
            return RedirectToAction("Register");
        }

        // 2. Validate password strength
        var passwordPattern = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        if (!Regex.IsMatch(Password, passwordPattern))
        {
            TempData["ErrorMessage"] = "Password must be at least 8 characters long, contain a letter, a number, and a special character.";
            return RedirectToAction("Register");
        }

        // 3. Check if email already exists
        if (_db.Companies.Any(c => c.Email == Email))
        {
            TempData["ErrorMessage"] = "Email is already registered.";
            return RedirectToAction("Register");
        }

        // 3. Hash password
        string hashedPassword = _passwordHasher.HashPassword(Email, Password);

        // 4. Save logo (optional)
        string logoPath = null;
        if (Logo != null && Logo.Length > 0)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Logo.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                Logo.CopyTo(stream);
            }
            logoPath = "/uploads/" + fileName;
        }

        // 5. Save to DB
        var company = new Company
        {
            CompanyName = CompanyName,
            Email = Email,
            PasswordHash = hashedPassword,
            LogoPath = logoPath
        };
        _db.Companies.Add(company);
        _db.SaveChanges();

        // 6. Success message
        TempData["SuccessMessage"] = "Registration successful! You can now login.";
        return RedirectToAction("Register");
    }

}
