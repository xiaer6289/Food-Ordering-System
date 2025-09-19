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
       public IActionResult FoodListing(
    string? search = null, 
    string categoryId = "All", 
    string SeatNo = null, 
    decimal? minPrice = null, 
    decimal? maxPrice = null)
{
    ViewBag.SearchContext = "Food";
    ViewBag.SearchPlaceholder = "Search by Food Name";

    search = search?.Trim() ?? "";
    ViewBag.SearchTerm = search;
    ViewBag.SeatNo = SeatNo;
    ViewBag.MinPrice = minPrice;
    ViewBag.MaxPrice = maxPrice;

    IQueryable<Food> foods = _db.Foods;

    // Filter by category
    if (!string.IsNullOrEmpty(categoryId) && categoryId != "All")
    {
        foods = foods.Where(f => f.CategoryId == categoryId);
    }

    // Filter by search term
    if (!string.IsNullOrEmpty(search))
    {
        foods = foods.Where(f => f.Name.Contains(search));
    }

    // Filter by price range
    if (minPrice.HasValue)
    {
        foods = foods.Where(f => f.Price >= minPrice.Value);
    }
    if (maxPrice.HasValue)
    {
        foods = foods.Where(f => f.Price <= maxPrice.Value);
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
