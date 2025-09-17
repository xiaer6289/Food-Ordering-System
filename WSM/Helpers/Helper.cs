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

        public class CartItem
        {
            public int Quantity { get; set; }
            public string ExtraDetail { get; set; } = "N/A";
        }

        // Get cart from session, key is string (FoodId)
        public Dictionary<string, CartItem> GetCart(string seatNo)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString(CartKey) ?? "{}";

            var allCarts = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CartItem>>>(cartJson)
                   ?? new Dictionary<string, Dictionary<string, CartItem>>();

            if (!allCarts.ContainsKey(seatNo))
                allCarts[seatNo] = new Dictionary<string, CartItem>();

            return allCarts[seatNo];
        }

        // Save cart to session
        public void SetCart(string seatNo, Dictionary<string, CartItem> cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString(CartKey) ?? "{}";

            var allCarts = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, CartItem>>>(cartJson)
                   ?? new Dictionary<string, Dictionary<string, CartItem>>();

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
                    .Select(f => f.Price * item.Value.Quantity)
                    .FirstOrDefault());
            return total;
        }

        public OrderDetail CreateOrderDetail(string seatNo = "0", string staffId = null, string adminId = null)
        {
            var cart = GetCart(seatNo);
            if (!cart.Any()) return null;

            string orderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var orderDetail = new OrderDetail
            {
                Id = orderId,
                SeatNo = int.Parse(seatNo),
                Quantity = cart.Sum(x => x.Value.Quantity),
                TotalPrice = CalculateTotal(seatNo),
                Status = "Pending",
                OrderDate = DateTime.Now,
                StaffId = staffId,
                AdminId = adminId,
                OrderItems = cart.Select((x, index) => new OrderItem
                {
                    Id = $"{orderId}-OI{index + 1}",
                    OrderDetailId = orderId,
                    FoodId = x.Key,
                    Quantity = x.Value.Quantity,
                    SubTotal = _db.Foods
                        .Where(f => f.Id == x.Key)
                        .Select(f => f.Price * x.Value.Quantity)
                        .FirstOrDefault(),
                    ExtraDetail = x.Value.ExtraDetail
                }).ToList()
            };

            _db.OrderDetails.Add(orderDetail);
            _db.SaveChanges();

            return orderDetail;
        }


    }
}
