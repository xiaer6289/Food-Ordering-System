using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSM.Models;
using System.Linq;

namespace WMS.Controllers
{
    [Authorize]
    public class MenuController : Controller
    {
        private readonly DB _db;

        public MenuController(DB db)
        {
            _db = db;
        }

        // GET: /Menu/FoodListing
        public IActionResult FoodListing(string search = null, string categoryId = "All", string SeatNo = null)
        {
            // Setup for search bar
            ViewBag.SearchContext = "Food";
            ViewBag.SearchPlaceholder = "Search by Food Name or ID";
            ViewBag.SearchTerm = search?.Trim() ?? "";
            ViewBag.SeatNo = SeatNo;

            IQueryable<Food> foods = _db.Foods;

            // Filter by category
            if (!string.IsNullOrEmpty(categoryId) && categoryId != "All")
            {
                foods = foods.Where(f => f.CategoryId == categoryId);
            }

            // Search by ID or Name
            if (!string.IsNullOrEmpty(search))
            {
                if (int.TryParse(search, out int foodId) && foods.Any(f => f.Id == foodId))
                {
                    foods = foods.Where(f => f.Id == foodId);
                }
                else
                {
                    foods = foods.Where(f => f.Name.Contains(search));
                }
            }

            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.SelectedCategory = categoryId;

            return View(foods.ToList());
        }

        public IActionResult FoodDetail(int id, string seatNo)
        {
            var food = _db.Foods.FirstOrDefault(f => f.Id == id);
            if (food == null) return NotFound();

            ViewBag.SeatNo = seatNo;
            return View(food);
        }
    }
}
