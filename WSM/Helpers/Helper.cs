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
        private const string CartKey = "Cart";


        public Helper(IHttpContextAccessor httpContextAccessor, DB db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        // Get cart from session, key is string (FoodId)
        public Dictionary<string, int> GetCart(string seatNo)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString(CartKey) ?? "{}";

            var allCarts = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(cartJson)
                           ?? new Dictionary<string, Dictionary<string, int>>();

            if (!allCarts.ContainsKey(seatNo))
                allCarts[seatNo] = new Dictionary<string, int>();

            return allCarts[seatNo];
        }

        // Save cart to session
        public void SetCart(string seatNo, Dictionary<string, int> cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString(CartKey) ?? "{}";

            var allCarts = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(cartJson)
                           ?? new Dictionary<string, Dictionary<string, int>>();

            allCarts[seatNo] = cart;

            session.SetString(CartKey, JsonSerializer.Serialize(allCarts));
        }


        // Calculate total price of current cart
        public decimal CalculateTotal(string seatNo)
        {
            var cart = GetCart(seatNo);
            decimal total = cart.Sum(item =>
                _db.Foods
                    .Where(f => f.Id == item.Key) 
                    .Select(f => f.Price * item.Value)
                    .FirstOrDefault());
            return total;
        }

        // Create OrderDetail and OrderItems based on current cart
        public OrderDetail CreateOrderDetail(string seatNo, string staffId = "S001")
        {
            var cart = GetCart(seatNo);
            if (!cart.Any()) return null;

            string orderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var orderDetail = new OrderDetail
            {
                Id = orderId,
                SeatNo = int.Parse(seatNo),
                Quantity = cart.Sum(x => x.Value),
                TotalPrice = CalculateTotal(seatNo),
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
