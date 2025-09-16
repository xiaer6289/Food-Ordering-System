using Microsoft.AspNetCore.Mvc;
using WSM.Models;
using System.Linq;

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
        var seats = _context.Seat.ToList();
        return View("Table", seats);
    }

    public IActionResult Create()
    {
        // Find the smallest available SeatNo starting from 1
        int newSeatNo = 1;
        var existing = _context.Seat.Select(s => s.SeatNo).OrderBy(n => n);
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

        _context.Seat.Add(new Seat { SeatNo = newSeatNo, Status = "Available" });
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    public IActionResult Delete()
    {
        var maxSeat = _context.Seat.OrderByDescending(s => s.SeatNo).FirstOrDefault();
        if (maxSeat != null)
        {
            _context.Seat.Remove(maxSeat);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
