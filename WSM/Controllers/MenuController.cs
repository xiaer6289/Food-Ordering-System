using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using X.PagedList;
using X.PagedList.Extensions;

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

        public IActionResult FoodListing(
            string? search = null,
            string categoryId = "All",
            string SeatNo = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int page = 1,              // <-- Add page parameter
            int pageSize = 6           // <-- Default page size
        )
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

            // Order by Name for consistency
            foods = foods.OrderBy(f => f.Name);

            // Apply paging
            var pagedFoods = foods.ToPagedList(page, pageSize);

            var topSellingFoods = _db.OrderItems
                .Include(oi => oi.Food)
                .GroupBy(oi => new { oi.FoodId, oi.Food.Name, oi.Food.Photo })
                .Select(g => new
                {
                    FoodId = g.Key.FoodId,
                    Name = g.Key.Name,
                    Photo = g.Key.Photo,
                    TotalSold = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(3)
                .ToList();

            ViewBag.TopSellingFoods = topSellingFoods;
            ViewBag.Categories = _db.Categories.ToList();
            ViewBag.SelectedCategory = categoryId;

            return View(pagedFoods);
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
