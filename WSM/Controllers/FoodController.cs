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
            // Get all foods including their categories
            var foods = db.Foods.Include(f => f.Category).AsQueryable();

            // Pass the current search string back to the view for display
            ViewData["CurrentFilter"] = searchString;

            // If there is a search term, filter the results
            if (!string.IsNullOrEmpty(searchString))
            {
                foods = foods.Where(f => f.Name.Contains(searchString) ||
                                         f.Description.Contains(searchString));
            }

            // Return filtered or full list
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
        public IActionResult CreateFood(WSM.Models.Food model)
        {
            //Check if input is valid
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                Console.WriteLine("Validation Errors: " + errors);
            }
            //Get last Food ID from database
            var lastFood = db.Foods
                             .OrderByDescending(f => f.Id)
                             .FirstOrDefault();
            //Generate new ID
            if (lastFood == null)
                model.Id = "F0001"; // first food
            else
            {
                int lastNum = int.Parse(lastFood.Id.Substring(1)); // remove 'F' and parse number
                model.Id = "F" + (lastNum + 1).ToString("D4"); // pad with zeros
            }
            //Add to database
            db.Foods.Add(model);
            db.SaveChanges();

            // 5️⃣ Redirect to Foods list
            return RedirectToAction(nameof(Foods));
        }
        // GET: Edit Food
        [HttpGet]
        public IActionResult EditFood(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var food = db.Foods.FirstOrDefault(f => f.Id == id);
            if (food == null)
                return NotFound();
            // Map database entity to ViewModel
            var model = new EditFoodVM
            {
                Id = food.Id,
                Name = food.Name,
                Price = food.Price,
                Description = food.Description,
                Image = food.Image,
                CategoryId = food.CategoryId
            };
            // Populate dropdown
            ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name", food.CategoryId);

            return View(model);
        }
        // POST: Edit Food
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFood(EditFoodVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            var existingFood = db.Foods.FirstOrDefault(f => f.Id == model.Id);
            if (existingFood == null)
                return NotFound();

            // If CategoryId is null or empty, keep the old category
            var finalCategoryId = string.IsNullOrEmpty(model.CategoryId) ? existingFood.CategoryId : model.CategoryId;

            // Update only editable fields
            existingFood.Name = model.Name;
            existingFood.Price = model.Price;
            existingFood.Description = model.Description;
            existingFood.Image = model.Image;
            existingFood.CategoryId = finalCategoryId;

            db.Foods.Update(existingFood);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Food updated successfully!";
            return RedirectToAction(nameof(Foods));
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


