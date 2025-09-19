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
        public IActionResult FoodListing(string? search = null, string categoryId = "All", string SeatNo = null)
        {
            ViewBag.SearchContext = "Food";
            ViewBag.SearchPlaceholder = "Search by Food Name";

            search = search?.Trim() ?? "";
            ViewBag.SearchTerm = search;
            ViewBag.SeatNo = SeatNo;

            IQueryable<Food> foods = _db.Foods;

            if (!string.IsNullOrEmpty(categoryId) && categoryId != "All")
            {
                foods = foods.Where(f => f.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                foods = foods.Where(f => f.Name.Contains(search));
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
