using Microsoft.AspNetCore.Http;
using System.Text.Json;
using WMS.Models;
using WSM.Models;

namespace WSM.Helpers
{
    public class Helper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DB _db;

        public Helper(IHttpContextAccessor httpContextAccessor, DB db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        // Get cart from session, key is string (FoodId)
        public Dictionary<string, int> GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString("Cart") ?? "{}";
            return JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson) ?? new Dictionary<string, int>();
        }

        // Save cart to session
        public void SetCart(Dictionary<string, int> cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString("Cart", JsonSerializer.Serialize(cart));
        }

        // Calculate total price of current cart
        public decimal CalculateTotal()
        {
            var cart = GetCart();
            decimal total = cart.Sum(item =>
                _db.Foods
                    .Where(f => f.Id.ToString() == item.Key)
                    .Select(f => f.Price * item.Value)
                    .FirstOrDefault());
            return total;
        }

        // Create OrderDetail and OrderItems based on current cart
        public OrderDetail CreateOrderDetail(string staffId = "S001")
        {
            var cart = GetCart();
            if (!cart.Any()) return null;

            string orderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var orderDetail = new OrderDetail
            {
                Id = orderId,
                SeatNo = 0,
                Quantity = cart.Sum(x => x.Value),
                TotalPrice = CalculateTotal(),
                Status = "Completed",
                OrderDate = DateTime.Now,
                StaffId = staffId,
                OrderItems = cart.Select((x, index) => new OrderItem
                {
                    Id = $"{orderId}-OI{index + 1}",
                    OrderDetailId = orderId,
                    FoodId = x.Key, // string key
                    Quantity = x.Value,
                    SubTotal = _db.Foods
                                .Where(f => f.Id.ToString() == x.Key)
                                .Select(f => f.Price * x.Value)
                                .FirstOrDefault(),
                    ExtraDetail = "N/A"
                }).ToList()
            };

            _db.OrderDetails.Add(orderDetail);
            _db.SaveChanges();
            return orderDetail;
        }
    }
}
