using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using System.IO;
using System.Linq;

namespace WSM.Controllers
{
    public class FoodController : Controller
    {
        private readonly DB db;
        private readonly IWebHostEnvironment _env;

        public FoodController(DB db, IWebHostEnvironment env)
        {
            this.db = db;
            _env = env;
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
        public IActionResult CreateFood(Food model)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", model.CategoryId);
                return View(model);
            }

            // Generate new Food ID
            var lastFood = db.Foods.OrderByDescending(f => f.Id).FirstOrDefault();
            model.Id = lastFood == null ? "F0001" : "F" + (int.Parse(lastFood.Id.Substring(1)) + 1).ToString("D4");

            // *** Handle File Upload ***
            if (model.Photo != null && model.Photo.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "foods");

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique file name
                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file to server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(stream);
                }

                // Save relative path to DB
                model.Image = "/uploads/foods/" + uniqueFileName;
            }
            else
            {
                // If no image is uploaded, you can either set a default image or leave it null
                model.Image = null;
            }

            db.Foods.Add(model);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Food created successfully!";
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

        public IActionResult FoodListing(string categoryId = "All")
        {
            var categories = db.Categories.ToList();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;

            IQueryable<WSM.Models.Food> foods = db.Foods;

            if (categoryId != "All")
                foods = foods.Where(f => f.CategoryId == categoryId);

            return View(foods.ToList());
        }


      

    }
}




