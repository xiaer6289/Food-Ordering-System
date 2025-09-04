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
            //Check if input is valid
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                Console.WriteLine("Validation Errors: " + errors);

                // Return the view with validation errors
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", model.CategoryId);
                return View(model);
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

        // GET: /Food/EditFood/{id}
        public IActionResult EditFood(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var food = db.Foods.Find(id);
            if (food == null) return NotFound();

            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", food.CategoryId);
            return View(food); // this looks for Views/Food/EditFood.cshtml
        }

        // POST: /Food/EditFood/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFood(Food model)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                Console.WriteLine("Validation Errors in EditFood: " + errors);

                // Return the view with validation errors
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", model.CategoryId);
                return View("EditFood", model);
            }

            try
            {
                var f = db.Foods.Find(model.Id);

                if (f == null)
                {
                    return RedirectToAction("Foods");
                }

                // Make sure the entity exists before updating
                var existingFood = db.Foods.Find(model.Id);
                if (existingFood == null)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    
                    f.Name = model.Name.Trim().ToLower();
                    f.Price = model.Price;
                    f.Description = model.Description?.Trim();
                    f.Image = model.Image?.Trim();
                    f.CategoryId = model.CategoryId;

                    db.SaveChanges();
                    TempData["Info"] = "Food updated.";
                    return RedirectToAction("Foods");
                }

                ViewBag.CategoryList = new SelectList(db.Categories, "Id", "Name");
                return View(model);

                //// Update the existing entity's properties
                //existingFood.Name = model.Name;
                //existingFood.Price = model.Price;
                //existingFood.Description = model.Description;
                //existingFood.Image = model.Image;
                //existingFood.CategoryId = model.CategoryId;

                //// Save changes
                //db.SaveChanges();

                //// Add success message (optional)
                //TempData["SuccessMessage"] = "Food item updated successfully!";

                //return RedirectToAction(nameof(Foods));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!db.Foods.Any(f => f.Id == model.Id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine("Error updating food: " + ex.Message);

                // Add error message
                ModelState.AddModelError("", "An error occurred while updating the food item. Please try again.");
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", model.CategoryId);
                return View("EditFood", model);
            }
        }

        // GET: /Food/DeleteFood/{id}
        public IActionResult DeleteFood(string id)
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