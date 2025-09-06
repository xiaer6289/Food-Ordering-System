using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace WSM.Controllers;

public class AdminController : Controller
{
    private readonly DB db;

    public AdminController(DB db)
    {
        this.db = db;
    }

    // GET: /Admin/Admins
    public IActionResult Admins(string searchString)
    {
        searchString = searchString?.Trim();
        var admins = db.Admins.AsQueryable();
        if (!string.IsNullOrEmpty(searchString))
        {
            admins = admins.Where(a => a.Name.Contains(searchString) || a.PhoneNo.Contains(searchString) || a.Email.Contains(searchString));
        }
        var model = admins.ToList();
        ViewData["CurrentFilter"] = searchString;
        return View(model);
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
        // Generate Id before validation to satisfy [Required] constraint
        model.Id = GenerateSequentialId();

        // Clear any ModelState errors for Id since it's auto-generated
        if (ModelState.ContainsKey("Id"))
        {
            ModelState["Id"].Errors.Clear();
            ModelState["Id"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
        }

        // Server-side validation for PhoneNo
        if (!string.IsNullOrEmpty(model.PhoneNo) && !Regex.IsMatch(model.PhoneNo, @"^01[0-9]{8,13}$"))
        {
            ModelState.AddModelError("PhoneNo", "Phone number must start with '01' and be 10 to 15 digits long.");
        }

        // Server-side validation for Password
        if (!string.IsNullOrEmpty(model.Password) && !Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,20}$"))
        {
            ModelState.AddModelError("Password", "Password must be 8 to 20 characters long, with at least one uppercase letter, one lowercase letter, one digit, and one special character (!@#$%^&*).");
        }

        // Server-side validation for Email
        if (!string.IsNullOrEmpty(model.Email) && !Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ModelState.AddModelError("Email", "Please enter a valid email address.");
        }

        if (ModelState.IsValid)
        {
            try
            {
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

   
    public IActionResult EditAdmin(string id)
    {
        var admin = db.Admins.Find(id);
        if (admin == null) return NotFound();
        return View(admin);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAdmin(Admin model)
    {
        // Server-side validation for PhoneNo
        if (!string.IsNullOrEmpty(model.PhoneNo) && !Regex.IsMatch(model.PhoneNo, @"^01[0-9]{8,13}$"))
        {
            ModelState.AddModelError("PhoneNo", "Phone number must start with '01' and be 10 to 15 digits long.");
        }

        // Server-side validation for Password
        if (!string.IsNullOrEmpty(model.Password) && !Regex.IsMatch(model.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,20}$"))
        {
            ModelState.AddModelError("Password", "Password must be 8 to 20 characters long, with at least one uppercase letter, one lowercase letter, one digit, and one special character (!@#$%^&*).");
        }

        // Server-side validation for Email
        if (!string.IsNullOrEmpty(model.Email) && !Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ModelState.AddModelError("Email", "Please enter a valid email address.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                db.Admins.Update(model);
                db.SaveChanges();
                return RedirectToAction("Admins");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Failed to update admin: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        return View(model);
    }

    // GET: /Admin/DeleteAdmin/{id}
    public IActionResult DeleteAdmin(string id)
    {
        var admin = db.Admins.Include(a => a.Staffs).FirstOrDefault(a => a.Id == id);
        if (admin == null) return NotFound();
        if (admin.Staffs.Any())
        {
            ModelState.AddModelError("", "Cannot delete admin because they are linked to staff records.");
            return View("Admins", db.Admins.ToList());
        }
        db.Admins.Remove(admin);
        db.SaveChanges();
        return RedirectToAction("Admins");
    }

    // Helper method to generate a sequential Id (A0001, A0002, etc.)
    private string GenerateSequentialId()
    {
        // Get all Ids that start with 'A' and have a numeric suffix
        var adminIds = db.Admins
            .Where(a => a.Id.StartsWith("A") && a.Id.Length == 5)
            .Select(a => a.Id)
            .ToList();

        int maxNumber = 0;
        foreach (var id in adminIds)
        {
            if (int.TryParse(id.Substring(1), out int number))
            {
                if (number > maxNumber)
                {
                    maxNumber = number;
                }
            }
        }

        int nextNumber = maxNumber + 1; // Increment the highest number found
        if (nextNumber > 9999)
        {
            throw new InvalidOperationException("Maximum number of admin IDs reached (A9999).");
        }

        return $"A{nextNumber:D4}";
    }
}