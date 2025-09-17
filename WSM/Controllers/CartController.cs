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
        var cart = _helper.GetCart(seatNo) ?? new Dictionary<string, Helper.CartItem>();
        var cartItems = new List<dynamic>();

        foreach (var item in cart)
        {
            var food = _db.Foods.FirstOrDefault(f => f.Id.ToString() == item.Key);
            if (food != null)
            {
                cartItems.Add(new { Food = food, Quantity = item.Value }); // Quantity 现在是 CartItem
            }
        }

        ViewBag.CartItems = cartItems;
        ViewBag.TotalAmount = _helper.CalculateTotal(seatNo);
        ViewBag.SeatNo = seatNo;
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
}