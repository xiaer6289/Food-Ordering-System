using System.Text.RegularExpressions;
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

    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }


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
            HttpContext.Session.SetString("CompanyId", company.Id);

           

            // Save logo in session
            if (!string.IsNullOrEmpty(company.LogoPath))
            {
                HttpContext.Session.SetString("CompanyLogo", company.LogoPath);
            }

            // Redirect based on first login
            if (company.IsFirstLogin)
                return RedirectToAction("FillCompanyProfile"); // first login → fill profile
            else
                return RedirectToAction("Both", "Home"); // normal login > dashboard/home
        }
        else
        {
            TempData["ErrorMessage"] = "Incorrect password. Please try again.";
            return RedirectToAction("Login");
        }
    }




    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string Owner, string CompanyName, string Email, string Password, string ConfirmPassword)
    {
        // 1. Validate passwords match
        if (Password != ConfirmPassword)
        {
            TempData["ErrorMessage"] = "Passwords do not match.";
            return RedirectToAction("Register");
        }

        // 2. Validate password strength
        string passwordPattern = @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
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

        // 4. Hash password
        string hashedPassword = _passwordHasher.HashPassword(Email, Password);

        // 5. Generate new CompanyId
        var lastCompany = _db.Companies
            .OrderByDescending(c => c.Id)
            .FirstOrDefault();

        string newCompanyId = "COM00001"; // default if no company yet

        if (lastCompany != null)
        {
            try
            {
                // Extract numeric part safely
                int lastNumber = int.Parse(lastCompany.Id.Substring(3));
                newCompanyId = "COM" + (lastNumber + 1).ToString("D5");
            }
            catch
            {
                // fallback in case Id is corrupted
                newCompanyId = "COM00001";
            }
        }

        // 5. Save to DB
        var company = new Company
        {
            Id = newCompanyId,
            Owner = Owner,
            CompanyName = CompanyName,
            Email = Email,
            PasswordHash = hashedPassword,
            LogoPath = null,
            Phone = " " ,
            Street = " ",
            City = "",
            State = "",
            Postcode = "",
        };

        _db.Companies.Add(company);
        _db.SaveChanges();

        

        // Get the last Admin Id in DB
        var lastAdmin = _db.Admins
            .OrderByDescending(a => a.Id)
            .FirstOrDefault();

        string newId = "A00001"; // default if no admin exists

        if (lastAdmin != null)
        {
            // Extract numeric part (after 'A')
            int lastNumber = int.Parse(lastAdmin.Id.Substring(1));
            newId = "A" + (lastNumber + 1).ToString("D5");
        }

        var admin = new Admin
        {
            Id = newId,
            Email = company.Email,
            Password = hashedPassword, // make sure column length is big enough!
            Name = company.Owner,
            PhoneNo = company.Phone,
            CompanyId = company.Id

        };

        _db.Admins.Add(admin);
        _db.SaveChanges();

        TempData["SuccessMessage"] = "Registration successful! You can now login.";
        return RedirectToAction("Login");
    }

    
    [HttpGet]
    public IActionResult FillCompanyProfile()
    {
        string? companyId = HttpContext.Session.GetString("CompanyId");
        if (string.IsNullOrEmpty(companyId)) return RedirectToAction("Login");

        var company = _db.Companies.Find(companyId); //  matches string type
        if (company == null) return RedirectToAction("Login");

        return View(company);
    }

    [HttpPost]
    public IActionResult FillCompanyProfile(IFormFile Logo,
    [Bind("Street,City,State,Postcode,Phone,Description")] Company updatedCompany)
    {
        string? companyId = HttpContext.Session.GetString("CompanyId");
        if (string.IsNullOrEmpty(companyId)) return RedirectToAction("Login");

        var company = _db.Companies.Find(companyId);
        if (company == null) return RedirectToAction("Login");

        // Update fields
        company.Street = updatedCompany.Street;
        company.City = updatedCompany.City;
        company.State = updatedCompany.State;
        company.Postcode = updatedCompany.Postcode;
        company.Phone = updatedCompany.Phone;
        company.Description = updatedCompany.Description;

        // Sync with admin phone
        var admin = _db.Admins.FirstOrDefault(a => a.Email == company.Email);
        if (admin != null)
        {
            admin.PhoneNo = company.Phone;
        }

        // Upload logo
        if (Logo != null && Logo.Length > 0)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(Logo.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                Logo.CopyTo(stream);
            }

            company.LogoPath = "/uploads/" + fileName;
        }

        company.IsFirstLogin = false;
        _db.SaveChanges();

        return RedirectToAction("Both", "Home");
    }

}
