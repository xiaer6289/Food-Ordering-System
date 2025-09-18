using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using WSM.Helpers;

namespace WSM.Controllers;

public class CartController : Controller
{
    private readonly Helper _helper;
    private readonly DB _db;

    public CartController(Helper helper, DB db)
    {
        _helper = helper;
        _db = db;
    }

    public IActionResult Cart(string seatNo)
    {
        var sessionCart = _helper.GetCart(seatNo) ?? new Dictionary<string, Helper.CartItem>();
        var cartItems = new List<dynamic>();
        foreach (var item in sessionCart)
        {
            var food = _db.Foods.FirstOrDefault(f => f.Id.ToString() == item.Key);
            if (food != null)
            {
                cartItems.Add(new { Food = food, Quantity = item.Value });
            }
        }


        var previousOrders = _db.OrderDetails
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Food)
            .Where(o => o.SeatNo == int.Parse(seatNo) && o.Status == "Preparing")
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        ViewBag.CartItems = cartItems;
        ViewBag.TotalAmount = _helper.CalculateTotal(seatNo);
        ViewBag.SeatNo = seatNo;
        ViewBag.PreviousOrders = previousOrders;

        return View();
    }

    [HttpPost]
    public IActionResult AddToCart(string seatNo, string foodId, int quantity, string extraDetail)
    {
        var cart = _helper.GetCart(seatNo);

        if (quantity <= 0)
        {
            cart.Remove(foodId);
        }
        else
        {
            if (cart.ContainsKey(foodId))
            {
                cart[foodId].Quantity += quantity;
                cart[foodId].ExtraDetail = extraDetail;
            }
            else
            {
                cart[foodId] = new Helper.CartItem
                {
                    Quantity = quantity,
                    ExtraDetail = extraDetail
                };
            }
        }

        _helper.SetCart(seatNo, cart);
        return RedirectToAction("Cart", new { seatNo });
    }


    public IActionResult Remove(string seatNo, string foodId)
    {
        var cart = _helper.GetCart(seatNo);
        cart.Remove(foodId);
        _helper.SetCart(seatNo, cart);
        return RedirectToAction("Cart", new { seatNo });
    }

    [HttpPost]
    public IActionResult SendOrder(string seatNo)
    {
        var role = HttpContext.Session.GetString("Role");
        var staffAdminId = HttpContext.Session.GetString("StaffAdminId");

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(staffAdminId))
            return Unauthorized("User not recognized as staff/admin.");

        OrderDetail orderDetail;

        if (role == "Staff")
            orderDetail = _helper.CreateOrderDetail(seatNo, staffId: staffAdminId);
        else if (role == "Admin")
            orderDetail = _helper.CreateOrderDetail(seatNo, adminId: staffAdminId);
        else
            return Unauthorized("Invalid role.");

        if (orderDetail == null)
            return BadRequest("Cart is empty.");

        orderDetail.Status = "Preparing";
        _db.OrderDetails.Update(orderDetail);

        var seat = _db.Seats.FirstOrDefault(s => s.SeatNo == int.Parse(seatNo));
        if (seat != null)
        {
            seat.Status = "Occupied";
            _db.Seats.Update(seat);
        }

        _db.SaveChanges();

        _helper.SetCart(seatNo, new Dictionary<string, Helper.CartItem>());

        return RedirectToAction("OrderSentConfirmation", new { orderId = orderDetail.Id });
    }


    public IActionResult OrderSentConfirmation(string orderId)
    {
        ViewBag.OrderId = orderId;
        return View();
    }

}