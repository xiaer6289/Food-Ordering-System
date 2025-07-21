using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers
{
    public class HomeController : Controller
    {
        private readonly DB db;

        public HomeController(DB db)
        {
            this.db = db;
        }

        // GET: /Home/Index
        public IActionResult Index()
        {

            return View();
        }

        // GET: /Home/Detail?id=F001
        public IActionResult Privacy()
        {
            

            return View();
        }

        public IActionResult Menu()
        {


            return View();
        }

        public IActionResult OrderHistory(string? id, string? sort, string? dir, int page = 1)
        {
            // For Layout.cshtml search bar
            ViewBag.SearchContext = "OrderHistory";
            ViewBag.SearchPlaceholder = "Search by Food ID/Name";

            // Searching
            ViewBag.Name = id = id?.Trim() ?? "";
            var searched = db.OrderDetails
                             .Where(s => s.Id.Contains(id));

            //sorting
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;

            Func<OrderDetail, object> fn = sort switch
            {
                "Id" => s => s.Id,
                "Name" => s => s.Foods,
                "Seat" => s => s.SeatNo,
                "Quantity" => s => s.Quantity,
                "status" => s => s.Status,
                "Total" => s => s.TotalPrice,
                "Date" => s => s.OrderDate,
                _ => s => s.Id
            };

            var sorted = dir == "des" ?
                     searched.OrderByDescending(fn) :
                     searched.OrderBy(fn);


            // (3) Paging ---------------------------
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
                return PartialView("OrderHistory", m);
            }


            return View(m);

        }



    }   
}
