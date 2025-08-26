using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using System.Linq;

namespace WSM.Controllers
{
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
            searchString = searchString?.Trim();

            var foods = db.Foods
                          .Include(f => f.Category)
                          .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                foods = foods.Where(f =>
                    f.Name.Contains(searchString) ||
                    f.Description.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View(foods.ToList());
        }

        // GET: /Food/CreateFood
        public IActionResult CreateFood()
        {
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        // POST: /Food/CreateFood
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFood(Food model)
        {
            // 1️⃣ Check if input is valid
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                Console.WriteLine("Validation Errors: " + errors);
            }



            // 2️⃣ Get last Food ID from database
            var lastFood = db.Foods
                             .OrderByDescending(f => f.Id)
                             .FirstOrDefault();

            // 3️⃣ Generate new ID
            if (lastFood == null)
                model.Id = "F0001"; // first food
            else
            {
                int lastNum = int.Parse(lastFood.Id.Substring(1)); // remove 'F' and parse number
                model.Id = "F" + (lastNum + 1).ToString("D4"); // pad with zeros
            }

            // 4️⃣ Add to database
            db.Foods.Add(model);
            db.SaveChanges();

            // 5️⃣ Redirect to Foods list
            return RedirectToAction(nameof(Foods));
        }

        // GET: /Food/Edit/{id}
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var food = db.Foods.Find(id);
            if (food == null) return NotFound();

            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", food.CategoryId);
            return View(food);
        }

        // POST: /Food/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Food model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Foods.Update(model);
                    db.SaveChanges();
                    return RedirectToAction(nameof(Foods));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!db.Foods.Any(f => f.Id == model.Id))
                        return NotFound();

                    throw;
                }
            }

            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: /Food/Delete/{id}
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var food = db.Foods.Find(id);
            if (food == null) return NotFound();

            db.Foods.Remove(food);
            db.SaveChanges();
            return RedirectToAction(nameof(Foods));
        }
    }
}