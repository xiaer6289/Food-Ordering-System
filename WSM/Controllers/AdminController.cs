using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;

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
        // Sanitize search string to prevent injection
        searchString = searchString?.Trim();

        var admins = db.Admins.AsQueryable();
        if (!string.IsNullOrEmpty(searchString))
        {
            admins = admins.Where(a => a.Name.Contains(searchString) || a.PhoneNo.Contains(searchString));
        }

        // Fetch the filtered list
        var model = admins.ToList();

        // Store search string for view to maintain state
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
        if (ModelState.IsValid)
        {
            model.Id = Guid.NewGuid();
            db.Admins.Add(model);
            db.SaveChanges();
            return RedirectToAction("Admins");
        }
        return View(model);
    }

    // GET: /Admin/EditAdmin/{id}
    public IActionResult EditAdmin(Guid id)
    {
        var admin = db.Admins.Find(id);
        if (admin == null) return NotFound();
        return View(admin);
    }

    // POST: /Admin/EditAdmin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAdmin(Admin model)
    {
        if (ModelState.IsValid)
        {
            db.Admins.Update(model);
            db.SaveChanges();
            return RedirectToAction("Admins");
        }
        return View(model);
    }

    // GET: /Admin/DeleteAdmin/{id}
    public IActionResult DeleteAdmin(Guid id)
    {
        var admin = db.Admins.Find(id);
        if (admin == null) return NotFound();
        db.Admins.Remove(admin);
        db.SaveChanges();
        return RedirectToAction("Admins");
    }

}