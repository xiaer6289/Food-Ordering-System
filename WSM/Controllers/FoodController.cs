using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using WSM.Helpers;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers
{
    public class FoodController : Controller
    {
        private readonly DB db;
        private readonly FoodHelper hp;

        public FoodController(DB db, FoodHelper hp)
        {
            this.db = db;
            this.hp = hp;
        }
        // GET: /Food/Foods
        public IActionResult Foods(string? id, string? sort, string? dir, int page = 1)
        {

            // For Layout.cshtml search bar
            ViewBag.SearchContext = "Food";
            ViewBag.SearchPlaceholder = "Search by Food ID/Name";

            // Searching
            ViewBag.Name = id = id?.Trim() ?? "";
            var searched = db.Foods.AsQueryable();

            if (!string.IsNullOrEmpty(id))
            {
                // Try parse id as integer for Id search
                if (int.TryParse(id, out int searchId))
                {
                    searched = searched.Where(s => s.Id == searchId || s.Name.Contains(id));
                }
                else
                {
                    searched = searched.Where(s => s.Name.Contains(id));
                }
            }


            //sorting
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            Func<Food, object> fn = sort switch
            {
                "Id" => s => s.Id,
                "Image" => s => s.Image,
                "Name" => s => s.Name,
                "Price" => s => s.Price,
                "Description" => s => s.Description,
                "Category" => s => s.Category,
                _ => s => s.Id
            };

            var sorted = dir == "des" ?
                    searched.OrderByDescending(fn) :
                    searched.OrderBy(fn);

            // Paging
            if (page < 1)
            {
                return RedirectToAction(null, new { id, sort, dir, page = 1 });
            }

            var m = sorted.ToPagedList(page, 10);

            if (page > m.PageCount && m.PageCount > 0)
            {
                return RedirectToAction(null, new { id, sort, dir, page = m.PageCount });
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Foods", m);
            }

            ViewBag.Foods = db.Foods.ToList();
            return View(m);
        }

        //GET: /Food/CreateFood
        public IActionResult CreateFood()
        {
            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
            return View();
        }

        // POST: /Food/CreateFood
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFood(FoodVM vm)
        {
            // Validate model
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", vm.CategoryId);
                return View(vm);
            }

            if (vm.Photo != null)
            {
                var e = hp.ValidatePhoto(vm.Photo);
                if (e != "") ModelState.AddModelError("Photo", e);
            }



            if (ModelState.IsValid)
            {
                String photoString = hp.SavePhoto(vm.Photo, "uploads");

                db.Foods.Add(new()
                {
                    Name = vm.Name,
                    Price = vm.Price,
                    Description = vm.Description,
                    Photo = photoString,
                    CategoryId = vm.CategoryId,
                });
                db.SaveChanges();
                TempData["Info"] = "Food Created.";
                return RedirectToAction("Foods");
            }

            return View(vm);
        }

        //    // Generate new Food ID
        //    var lastFood = db.Foods.OrderByDescending(f => f.Id).FirstOrDefault();
        //    model.Id = lastFood == null ? "F0001" : "F" + (int.Parse(lastFood.Id.Substring(1)) + 1).ToString("D4");

        //    // *** Handle File Upload ***
        //    if (model.Photo != null && model.Photo.Length > 0)
        //    {
        //        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "foods");

        //        // Create directory if it doesn't exist
        //        if (!Directory.Exists(uploadsFolder))
        //            Directory.CreateDirectory(uploadsFolder);

        //        // Generate unique file name
        //        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);

        //        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //        // Save file to server
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            model.Photo.CopyTo(stream);
        //        }

        //        // Save relative path to DB
        //        model.Image = "/uploads/foods/" + uniqueFileName;
        //    }
        //    else
        //    {
        //        // If no image is uploaded, you can either set a default image or leave it null
        //        model.Image = null;
        //    }

        //    db.Foods.Add(model);
        //    db.SaveChanges();

        //    TempData["SuccessMessage"] = "Food created successfully!";
        //    return RedirectToAction(nameof(Foods));

        // GET: /Food/EditFood/{id}
        public IActionResult EditFood(int id)
        {
            var food = db.Foods.FirstOrDefault(f => f.Id == id);
            if (food == null) return NotFound();

            var vm = new EditFoodVM
            {
                Id = food.Id,
                Name = food.Name,
                Price = food.Price,
                Description = food.Description,
                Image = food.Photo,
                CategoryId = food.CategoryId
            };

            ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", food.CategoryId);
            return View(vm);
        }

        // POST: /Food/EditFood
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFood(EditFoodVM vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", vm.CategoryId);
                return View(vm);
            }

            var existingFood = db.Foods.FirstOrDefault(f => f.Id == vm.Id);
            if (existingFood == null) return NotFound();

            if (vm.Photo != null)
            {
                var error = hp.ValidatePhoto(vm.Photo);
                if (!string.IsNullOrEmpty(error))
                {
                    ModelState.AddModelError("Photo", error);
                    ViewBag.Categories = new SelectList(db.Categories, "Id", "Name", vm.CategoryId);
                    return View(vm);
                }
            }

            existingFood.Name = vm.Name;
            existingFood.Price = vm.Price;
            existingFood.Description = vm.Description;
            existingFood.CategoryId = string.IsNullOrEmpty(vm.CategoryId) ? existingFood.CategoryId : vm.CategoryId;

            if (vm.Photo != null)
            {
                var photoString = hp.SavePhoto(vm.Photo, "uploads");
                existingFood.Photo = photoString;
            }

            db.Foods.Update(existingFood);
            db.SaveChanges();

            TempData["Info"] = "Food updated successfully!";
            return RedirectToAction("Foods");
        }
        // GET: /Food/DeleteFood
        public IActionResult DeleteFood(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!int.TryParse(id, out int foodId)) return NotFound();
            var food = db.Foods.Find(foodId);
            if (food == null) return NotFound();
            db.Foods.Remove(food);
            db.SaveChanges();
            return RedirectToAction(nameof(Foods));
        }

        public IActionResult FoodListing(string categoryId = "All")
        {
            // Fetch all categories for filter buttons
            var categories = db.Categories.ToList();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;

            // Start with all foods
            IQueryable<Food> foods = db.Foods;

            // Filter by category if it's not "All"
            if (!string.IsNullOrEmpty(categoryId) && categoryId != "All")
            {
                foods = foods.Where(f => f.CategoryId == categoryId);
            }

            return View(foods.ToList());
        }

    }
}




