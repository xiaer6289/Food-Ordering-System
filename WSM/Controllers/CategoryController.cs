using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace WSM.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly DB _db;

        public CategoryController(DB db)
        {
            _db = db;
        }

        [Authorize]
        public IActionResult Categories(
     string searchString,
     string sort = null,
     string dir = "asc",
     int page = 1
 )
        {
            var query = _db.Categories.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Name.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;

            if (!string.IsNullOrEmpty(sort))
            {
                query = sort switch
                {
                    "Name" => dir == "asc" ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name),
                    "Id" => dir == "asc" ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id),
                    _ => query
                };
            }

            var pagedList = query
                .Select(c => new CategoryVM
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToPagedList(page, 5);

            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            return View("Categories", pagedList);
        }
        public IActionResult CreateCategory()
        {
            return View("CreateCategory", new CategoryVM());
        }

        [HttpPost]
        public IActionResult CreateCategory(CategoryVM model)
        {
            if (ModelState.IsValid)
            {
                model.Id = model.Id?.ToUpper();

                bool idExists = _db.Categories.Any(c => c.Id == model.Id);

                if (idExists)
                {
                    ModelState.AddModelError("Id", "This Category ID already exists. Please use a different ID.");
                    return View("CreateCategory", model);
                }

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
            if (!ModelState.IsValid)
            {
                return View("EditCategory", model);
            }

            var category = _db.Categories.FirstOrDefault(c => c.Id == model.Id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = model.Name;
            _db.SaveChanges();

            return RedirectToAction("Categories");
        }
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

        [HttpPost]
        public IActionResult DeleteMany([FromForm] string[] ids)
        {
            if (ids != null && ids.Length > 0)
            {
                var categories = _db.Categories.Where(c => ids.Contains(c.Id)).ToList();
                if (categories.Any())
                {
                    _db.Categories.RemoveRange(categories);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("Categories");
        }


    }
}
