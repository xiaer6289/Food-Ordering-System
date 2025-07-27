using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using WSM.Models;
using X.PagedList.Extensions;

namespace WSM.Controllers;

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
        // For Layout.cshtml search bar
        ViewBag.SearchContext = "Restock";
        ViewBag.SearchPlaceholder = "Search";
        return View();
    }

    // GET: /Home/Detail?id=F001

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
            //"Name" => s => s.Foods,
            //"Seat" => s => s.SeatNo,
            //"Quantity" => s => s.Quantity,
            //"status" => s => s.Status,
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

    public IActionResult Detail(string? id)
    {
        var m = db.OrderDetails.Find(id);

        if (m == null)
        {
            return RedirectToAction("OrderHistory");
        }

        return View(m);
    }

    public IActionResult Restock(string? id, string? sort, string? dir, int page = 1)
    {
        // For Layout.cshtml search bar
        ViewBag.SearchContext = "Restock";
        ViewBag.SearchPlaceholder = "Search by Ingredient ID/Name";

        // Searching
        ViewBag.Name = id = id?.Trim() ?? "";
        var searched = db.Ingredients
                      .Where(s => s.Id.Contains(id) || s.Name.Contains(id));

        //sorting
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<Ingredient, object> fn = sort switch
        {
            "Id" => s => s.Id,
            "Name" => s => s.Name,
            "Quantity" => s => s.Quantity,
            "Weight(kg)" => s => s.Kilogram,
            "Price" => s => s.Price,
            "Total" => s => s.TotalPrice,
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
            return PartialView("Restock", m);
        }

        ViewBag.Ingredients = db.Ingredients.ToList();
        return View(m);

    }


    public IActionResult PurchaseOrder() //insert
    {
        return View();
    }

    public IActionResult Edit()
    {
        return View();
    }
}






