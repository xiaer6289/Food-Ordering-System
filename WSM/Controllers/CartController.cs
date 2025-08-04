using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WSM.Models;
using WSM.Helpers;

namespace WSM.Controllers;

public class CartController : Controller
{
    private readonly DB db;
    private readonly Helper hp;

    // Constructor to inject DB and Helper


    public CartController(DB db, Helper hp)
    {
        this.db = db;
        this.hp = hp;
    }

    // GET: /Cart/FakeOrder
    // Display the food list and pass the current cart to the view
    public IActionResult FakeOrder()
    {
        
        var foodList = db.Foods
            .Include(f => f.Category) 
            .ToList();

        var cart = hp.GetCart();

        var cartItems = cart.Select(item => new
        {
            Food = db.Foods.FirstOrDefault(f => f.Id == item.Key),
            Quantity = item.Value
        }).Where(item => item.Food != null).ToList();

        var totalAmount = cartItems.Sum(item => item.Food.Price * item.Quantity);

        ViewBag.Cart = cart;
        ViewBag.CartItems = cartItems;
        ViewBag.TotalAmount = totalAmount;

        return View(foodList);
    }

    // POST: /Cart/UpdateCart
    // Add or update food item in the cart
    [HttpPost]
    public IActionResult UpdateCart(string foodId, int quantity)
    {
        if (!db.Foods.Any(f => f.Id == foodId))
        {
            return BadRequest("Invalid Food ID"); 
        }

        var cart = hp.GetCart();

        if (quantity >= 1 && quantity <= 100)
        {
            cart[foodId] = quantity; 
        }
        else
        {
            cart.Remove(foodId); 
        }

        hp.SetCart(cart);

        return RedirectToAction("FakeOrder");
    }
}