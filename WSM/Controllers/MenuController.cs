using Microsoft.AspNetCore.Mvc;
using WSM.Helpers;
using WSM.Models;

namespace WMS.Controllers
{
    public class MenuController : Controller
    {
        private readonly DB _db;

        public MenuController(DB db)
        {
            _db = db;
        }
        public IActionResult FoodListing(string categoryId = "All", string SeatNo = null)
        {
            var categories = _db.Categories.ToList();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SeatNo = SeatNo;

            IQueryable<WSM.Models.Food> foods = _db.Foods;

            if (categoryId != "All")
                foods = foods.Where(f => f.CategoryId == categoryId);

            return View(foods.ToList());
        }


        public IActionResult FoodDetail(string id, string seatNo)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            if (!int.TryParse(id, out int foodId))
                return NotFound();

            var food = _db.Foods
                          .Where(f => f.Id == foodId)
                          .FirstOrDefault();

            if (food == null)
                return NotFound();

            ViewBag.SeatNo = seatNo;
            return View(food);
        }

    }
}
