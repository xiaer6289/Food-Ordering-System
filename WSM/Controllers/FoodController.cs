using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;

namespace WSM.Controllers;

public class FoodController : Controller
{
    private readonly DB db;

    public FoodController(DB db)
    {
        this.db = db;
    }

    // GET: /Food/Foods
    public IActionResult Foods(string searchString)
    {
        // Sanitize search string to prevent injection
        searchString = searchString?.Trim();

        var foods = db.Foods.Include(f => f.Category).AsQueryable();
        if (!string.IsNullOrEmpty(searchString))
        {
            foods = foods.Where(f => f.Name.Contains(searchString) || f.Description.Contains(searchString));
        }

        // Fetch the filtered list
        var model = foods.ToList();

        // Store search string for view to maintain state
        ViewData["CurrentFilter"] = searchString;

        return View(model);
    }

    // GET: /Food/CreateFood
    public IActionResult CreateFood()
    {
        ViewBag.Categories = db.Categories.ToList();
        return View();
    }

    // POST: /Food/CreateFood
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateFood(Food model)
    {
        if (ModelState.IsValid)
        {
            db.Foods.Add(model);
            db.SaveChanges();
            return RedirectToAction("Foods");
        }
        ViewBag.Categories = db.Categories.ToList();
        return View(model);
    }

    // GET: /Food/EditFood/{id}
    public IActionResult EditFood(string id)
    {
        var food = db.Foods.Find(id);
        if (food == null) return NotFound();
        ViewBag.Categories = db.Categories.ToList();
        return View(food);
    }

    // POST: /Food/EditFood
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditFood(Food model)
    {
        if (ModelState.IsValid)
        {
            db.Foods.Update(model);
            db.SaveChanges();
            return RedirectToAction("Foods");
        }
        ViewBag.Categories = db.Categories.ToList();
        return View(model);
    }

    // GET: /Food/DeleteFood/{id}
    public IActionResult DeleteFood(string id)
    {
        var food = db.Foods.Find(id);
        if (food == null) return NotFound();
        db.Foods.Remove(food);
        db.SaveChanges();
        return RedirectToAction("Foods");
    }
}