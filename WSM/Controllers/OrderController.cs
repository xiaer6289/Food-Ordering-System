using Microsoft.AspNetCore.Mvc;
using WMS.Models;
using WSM.Helpers;
using WSM.Models;

namespace WSM.Controllers
{
    public class OrderController : Controller
    {
        private readonly Helper _helper;
        private readonly DB _db;

        public OrderController(Helper helper, DB db)
        {
            _helper = helper;
            _db = db;
        }

        public IActionResult Cart()
        {

            ViewBag.Cart = _helper.GetCart() ?? new Dictionary<string, int>();

            var foods = _db.Foods.ToList();

            return View(foods);
        }


        // Update cart with foodId and quantity
        [HttpPost]
        public IActionResult UpdateCart(string foodId, int quantity)
        {
            var cart = _helper.GetCart();

            if (quantity <= 0)
                cart.Remove(foodId);
            else
                cart[foodId] = quantity;

            _helper.SetCart(cart);
            return RedirectToAction("FoodDetail");
        }
    }
}
