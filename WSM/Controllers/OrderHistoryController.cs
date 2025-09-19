using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using X.PagedList.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WMS.Controllers;

[Authorize]
public class OrderHistoryController : Controller
{
    private readonly DB db;

    public OrderHistoryController(DB db)
    {
        this.db = db;
    }
    [Authorize]
    [Authorize(Roles = "Staff")]
    [Authorize(Roles = "Admin")]
    public IActionResult OrderHistory(string? id, string? sort, string? dir, string? status, int page = 1)
    {
        // For Layout.cshtml search bar
        ViewBag.SearchContext = "OrderHistory";
        ViewBag.SearchPlaceholder = "Search by Order ID";

        // Searching
        ViewBag.Name = id = id?.Trim() ?? "";
        var searched = db.OrderDetails
                         .Include(o => o.Payment)
                         .Where(s => s.Id.Contains(id));

        // Filtering
        ViewBag.Status = status;
        if (!string.IsNullOrEmpty(status) && status != "--All--")
        {
            searched = searched.Where(s => s.Status == status);
        }

        //sorting
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        Func<OrderDetail, object> fn = sort switch
        {
            "Id" => s => s.Id,
            "Total" => s => s.TotalPrice,
            "Date" => s => s.OrderDate,
            "Status" => s => s.Status switch
            {
                "Paid" => 1,
                "Pending" => 2,
                "Partially Refunded" => 3,
                "Refunded" => 4,
                _ => 5
            },
            _ => s => s.Id
        };

        var sorted = dir == "des" ?
                 searched.OrderByDescending(fn) :
                 searched.OrderBy(fn);

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

    [Authorize(Roles = "Admin")]
    public IActionResult Detail(string? id)
    {
        var order = db.OrderDetails
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
            .Include(o => o.Payment)
            .FirstOrDefault(o => o.Id == id);

        if (order == null)
        {
            return RedirectToAction("OrderHistory");
        }

        var company = db.Companies.FirstOrDefault();

        var vm = new DetailVM
        {
            orderDetail = order,
            company = company
        };

        return View(vm);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Refund(string? id)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction("OrderHistory");
        var order = db.OrderDetails
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
            .Include(o => o.Payment)
            .FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction("OrderHistory");
        }

        var company = db.Companies.FirstOrDefault();

        var vm = new DetailVM
        {
            orderDetail = order,
            company = company
        };

        return View(vm);
    }

    [Authorize(Roles = "Admin")]
    // POST: Refund selected items
    [HttpPost]
    public IActionResult Refund(string orderId, string[] itemIds)
    {
        if (string.IsNullOrEmpty(orderId))
        {
            TempData["Error"] = "Order ID missing.";
            return RedirectToAction("OrderHistory");
        }
        var order = db.OrderDetails
            .Include(o => o.OrderItems)
            .FirstOrDefault(o => o.Id == orderId);
        if (order == null)
        {
            TempData["Error"] = "Order not found.";
            return RedirectToAction("OrderHistory");
        }
        if (itemIds == null || itemIds.Length == 0)
        {
            TempData["Error"] = "Please select items to refund.";
            return RedirectToAction("Refund", new { id = orderId });
        }
        // Set selected items' SubTotal to 0
        foreach (var item in order.OrderItems.Where(x => itemIds.Contains(x.Id)))
        {
            item.SubTotal = 0;
        }
        // Recalculate order total
        order.TotalPrice = order.OrderItems.Sum(x => x.SubTotal);
        // Update status
        order.Status = order.OrderItems.All(x => x.SubTotal == 0) ? "Refunded" : "Partially Refunded";
        db.SaveChanges();
        TempData["Info"] = $"Successfully refunded {itemIds.Length} item(s).";
        return RedirectToAction("OrderHistory");
    }
}



