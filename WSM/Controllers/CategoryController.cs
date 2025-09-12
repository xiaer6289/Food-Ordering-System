using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using System.Linq;

namespace WSM.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DB _db;

        public CategoryController(DB db)
        {
            _db = db;
        }

        // =============================
        // LIST (Categories.cshtml)
        // =============================
        public IActionResult Categories(string searchString)
        {
            var categories = _db.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            // Map entity -> VM
            var vmList = categories
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToList();

            return View("Categories", vmList);
        }

        // =============================
        // CREATE (CreateCategory.cshtml)
        // =============================
        public IActionResult CreateCategory()
        {
            return View("CreateCategory");
        }

        [HttpPost]
        public IActionResult CreateCategory(CategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var entity = new Category
                {
                    Id = model.Id,
                    Name = model.Name
                };

                _db.Categories.Add(entity);
                _db.SaveChanges();

                return RedirectToAction("Categories");
            }
            return View("CreateCategory", model);
        }

        // =============================
        // EDIT (EditCategory.cshtml)
        // =============================
        public IActionResult EditCategory(string id)
        {
            var category = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();

            var vm = new CategoryVM
            {
                Id = category.Id,
                Name = category.Name
            };

            return View("EditCategory", vm);
        }

        [HttpPost]
        public IActionResult EditCategory(CategoryVM model)
        {
            if (ModelState.IsValid)
            {
                var category = _db.Categories.FirstOrDefault(c => c.Id == model.Id);
                if (category == null) return NotFound();

                category.Name = model.Name;

                _db.Categories.Update(category);
                _db.SaveChanges();

                return RedirectToAction("Categories");
            }
            return View("EditCategory", model);
        }

        // =============================
        // DELETE
        // =============================
        [HttpPost]
        public IActionResult DeleteConfirmed(string id)
        {
            var category = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _db.Categories.Remove(category);
                _db.SaveChanges();
            }

            return RedirectToAction("Categories");
        }
    }
}
