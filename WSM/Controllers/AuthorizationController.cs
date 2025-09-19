using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WSM.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
namespace WSM.Services;



public class AuthorizationController : Controller
{
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly DB _db;
    private readonly IWebHostEnvironment _env;
    private readonly IEmailSender _emailSender;



    public AuthorizationController(DB db, IPasswordHasher<string> passwordHasher, IWebHostEnvironment env, IEmailSender emailSender)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _env = env;
        _emailSender = emailSender;
    }


    [HttpGet]
    public IActionResult Login()
    {
        
        if (_db.Companies.Any())
        {
            
            return RedirectToAction("Both", "Home");
        }

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
            var admin = _db.Admins.FirstOrDefault(a => a.CompanyId == company.Id);
            if (admin != null)
            {
                HttpContext.Session.SetString("AdminId", admin.Id);
            }


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
        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        if (!Regex.IsMatch(Password, passwordPattern))
        {
            TempData["ErrorMessage"] = "Password must be at least 8 characters long, contain an uppercase letter, a lowercase letter, a number, and a special character.";
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
            Phone = "" ,
            Street = " ",
            City = "",
            State = "",
            Postcode = "",
            IsFirstLogin = true,
        };

        _db.Companies.Add(company);
        _db.SaveChanges();

        

        // Get the last Admin Id in DB
        var lastAdmin = _db.Admins
            .Where(a => a.Id.StartsWith("A") && a.Id.Length == 6)
            .OrderByDescending(a => Convert.ToInt32(a.Id.Substring(1, 5)))
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

        
        HttpContext.Session.SetString("CompanyId", company.Id);
        HttpContext.Session.SetString("AdminId", admin.Id);

        if (!string.IsNullOrEmpty(company.LogoPath))
        {
            HttpContext.Session.SetString("CompanyLogo", company.LogoPath);
        }

        // Redirect directly to FillCompanyProfile
        return RedirectToAction("FillCompanyProfile");
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

        // === Validation ===
        if (string.IsNullOrWhiteSpace(updatedCompany.Street))
        {
            TempData["ErrorMessage"] = "Street is required.";
            return RedirectToAction("FillCompanyProfile");
        }

        if (string.IsNullOrWhiteSpace(updatedCompany.City))
        {
            TempData["ErrorMessage"] = "City is required.";
            return RedirectToAction("FillCompanyProfile");
        }

        if (string.IsNullOrWhiteSpace(updatedCompany.State))
        {
            TempData["ErrorMessage"] = "State is required.";
            return RedirectToAction("FillCompanyProfile");
        }

        if (string.IsNullOrWhiteSpace(updatedCompany.Postcode))
        {
            TempData["ErrorMessage"] = "Postcode is required.";
            return RedirectToAction("FillCompanyProfile");
        }

        if (!Regex.IsMatch(updatedCompany.Postcode, @"^\d+$"))
        {
            TempData["ErrorMessage"] = "Postcode must be numeric.";
            return RedirectToAction("FillCompanyProfile");
        }

        if (string.IsNullOrWhiteSpace(updatedCompany.Phone))
        {
            TempData["ErrorMessage"] = "Phone number is required.";
            return RedirectToAction("FillCompanyProfile");
        }

        string phonePattern = @"^\+?\d{8,15}$"; 
        if (!Regex.IsMatch(updatedCompany.Phone, phonePattern))
        {
            TempData["ErrorMessage"] = "Phone number format is invalid (eg.+60123456789).";
            return RedirectToAction("FillCompanyProfile");
        }

        // === Update fields ===
        company.Street = updatedCompany.Street;
        company.City = updatedCompany.City;
        company.State = updatedCompany.State;
        company.Postcode = updatedCompany.Postcode;
        company.Phone = updatedCompany.Phone;

        // If description empty, set as "N/A" or leave empty
        company.Description = string.IsNullOrWhiteSpace(updatedCompany.Description) ? "" : updatedCompany.Description;

        // === Sync with admin phone ===
        var admin = _db.Admins.FirstOrDefault(a => a.Email == company.Email);
        if (admin != null)
        {
            admin.PhoneNo = company.Phone;
        }

       
        if (Logo == null || Logo.Length == 0)
        {
            TempData["ErrorMessage"] = "Please upload a company logo.";
            return RedirectToAction("FillCompanyProfile");
        }

   
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
        HttpContext.Session.SetString("CompanyId", company.Id);

        // Save logo in session
        if (!string.IsNullOrEmpty(company.LogoPath))
        {
            HttpContext.Session.SetString("CompanyLogo", company.LogoPath);
        }

        company.IsFirstLogin = false;
        _db.SaveChanges();

        TempData["SuccessMessage"] = "Company registered successfully.";
        return RedirectToAction("Both", "Home");
    }



    //Forgot password
    [HttpGet]
    public IActionResult ForgotPassword(string role)
    {
        ViewBag.Role = role; // Pass role to view
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string Email, string role)
    {
        var company = _db.Companies.FirstOrDefault(c => c.Email == Email);
        var admin = _db.Admins.FirstOrDefault(a => a.Email == Email);
        var staff = _db.Staff.FirstOrDefault(s => s.Email == Email);

        if (company == null && admin == null && staff == null)
        {
            ViewBag.ErrorMessage = "Email not found.";
            return View();
        }

        string resetToken = Guid.NewGuid().ToString();
        DateTime expiry = DateTime.Now.AddHours(1);

        if (company != null)
        {
            company.ResetToken = resetToken;
            company.TokenExpiry = expiry;
        }
        else if (admin != null)
        {
            admin.ResetToken = resetToken;
            admin.TokenExpiry = expiry;
        }
        else if (staff != null)
        {
            staff.ResetToken = resetToken;
            staff.TokenExpiry = expiry;
        }
        _db.SaveChanges();

        string resetLink = Url.Action("ResetPassword", "Authorization",
            new { token = resetToken, email = Email, role = role }, Request.Scheme);

        string subject = "Password Reset";
        string body = $"<p>Click the link below to reset your password:</p>" +
                      $"<a href='{resetLink}'>Reset Password</a>";

        await _emailSender.SendEmailAsync(Email, subject, body);

        return RedirectToAction("ResetPasswordConfirm");
    }

  
    public IActionResult ResetPassword(string token, string email, string role)
    {
        var company = _db.Companies.FirstOrDefault(c => c.Email == email && c.ResetToken == token && c.TokenExpiry > DateTime.Now);
        var admin = _db.Admins.FirstOrDefault(a => a.Email == email && a.ResetToken == token && a.TokenExpiry > DateTime.Now);
        var staff = _db.Staff.FirstOrDefault(s => s.Email == email && s.ResetToken == token && s.TokenExpiry > DateTime.Now);

        if (company == null && admin == null && staff == null)
        {
            TempData["ErrorMessage"] = "Invalid or expired token.";
            return RedirectToAction("Login", role == "company" ? "Authorization" : "Home");
        }

        ViewBag.Email = email;
        ViewBag.Token = token;
        ViewBag.Role = role;
        return View();
    }

    [HttpPost]
    public IActionResult ResetPassword(string token, string email, string newPassword, string confirmPassword, string role)
    {
        if (newPassword != confirmPassword)
        {
            TempData["ErrorMessage"] = "Passwords do not match.";
            ViewBag.Email = email;
            ViewBag.Token = token;
            ViewBag.Role = role;
            return View();
        }

        string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        if (!Regex.IsMatch(newPassword, passwordPattern))
        {
            TempData["ErrorMessage"] = "Password must meet complexity requirements.";
            ViewBag.Email = email;
            ViewBag.Token = token;
            ViewBag.Role = role;
            return View();
        }

        // Fetch all accounts linked to the email
        var company = _db.Companies.FirstOrDefault(c => c.Email == email && c.ResetToken == token && c.TokenExpiry > DateTime.Now);
        var admin = _db.Admins.FirstOrDefault(a => a.Email == email && a.ResetToken == token && a.TokenExpiry > DateTime.Now);
        var staff = _db.Staff.FirstOrDefault(s => s.Email == email && s.ResetToken == token && s.TokenExpiry > DateTime.Now);

        if (company == null && admin == null && staff == null)
        {
            TempData["ErrorMessage"] = "Invalid or expired token.";
            return RedirectToAction("ForgotPassword", new { role = role });
        }

        string hashedPassword = _passwordHasher.HashPassword(email, newPassword);

        // Update company password and sync with admin
        if (company != null)
        {
            company.PasswordHash = hashedPassword;
            company.ResetToken = null;
            company.TokenExpiry = null;

            var linkedAdmin = _db.Admins.FirstOrDefault(a => a.Email == company.Email);
            if (linkedAdmin != null)
            {
                linkedAdmin.Password = hashedPassword;
                linkedAdmin.ResetToken = null;
                linkedAdmin.TokenExpiry = null;
            }
        }

        // If admin only exists without company
        if (admin != null && company == null)
        {
            admin.Password = hashedPassword;
            admin.ResetToken = null;
            admin.TokenExpiry = null;

            var linkedCompany = _db.Companies.FirstOrDefault(c => c.Email == admin.Email);
            if (linkedCompany != null)
            {
                linkedCompany.PasswordHash = hashedPassword;
                linkedCompany.ResetToken = null;
                linkedCompany.TokenExpiry = null;
            }
        }

        // Update staff normally
        if (staff != null)
        {
            staff.Password = hashedPassword;
            staff.ResetToken = null;
            staff.TokenExpiry = null;
        }

        _db.SaveChanges();
        TempData["SuccessMessage"] = "Password reset successfully.";

        if (role == "company")
            return RedirectToAction("Login", "Authorization");
        else
        {
            // Restore company session for admin/staff
            string? linkedCompanyId = null;

            if (company != null)
                linkedCompanyId = company.Id;
            else if (admin != null)
                linkedCompanyId = admin.CompanyId;
            else if (staff != null)
                linkedCompanyId = staff.CompanyId;

            if (!string.IsNullOrEmpty(linkedCompanyId))
            {
                HttpContext.Session.SetString("CompanyId", linkedCompanyId);

                var linkedCompany = _db.Companies.Find(linkedCompanyId);
                if (linkedCompany != null && !string.IsNullOrEmpty(linkedCompany.LogoPath))
                {
                    HttpContext.Session.SetString("CompanyLogo", linkedCompany.LogoPath);
                }
            }
        }
        TempData["SuccessMessage"] = "Password reset successfully.";
        return RedirectToAction("Both", "Home");
    }

    public IActionResult Reset()
    {
        return View();
    }

    public IActionResult ResetPasswordConfirm()
    {
        return View();
    }

}
