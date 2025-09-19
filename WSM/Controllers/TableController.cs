using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WSM.Models;

namespace WSM.Controllers;

[Authorize]
public class TableController : Controller
{
    private readonly DB _context;

    public TableController(DB context)
    {
        _context = context;
    }

    // Show all tables
    public IActionResult Index()
    {
        var seats = _context.Seats.ToList();
        return View("Table", seats);
    }

    [Authorize]
    public IActionResult Create()
    {
        // Find the smallest available SeatNo starting from 1
        int newSeatNo = 1;
        var existing = _context.Seats.Select(s => s.SeatNo).OrderBy(n => n);
        foreach (var n in existing)
        {
            if (n == newSeatNo)
            {
                newSeatNo++;
            }
            else
            {
                break;
            }
        }

        _context.Seats.Add(new Seat { SeatNo = newSeatNo, Status = "Available" });
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [Authorize]
    public IActionResult Delete()
    {
        var maxSeat = _context.Seats.OrderByDescending(s => s.SeatNo).FirstOrDefault();
        if (maxSeat != null)
        {
            _context.Seats.Remove(maxSeat);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
