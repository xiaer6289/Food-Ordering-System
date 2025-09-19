using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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
            {
                if (int.TryParse(item.Key, out int foodId))
                {
                    return _db.Foods
                        .Where(f => f.Id == foodId)
                        .Select(f => f.Price * item.Value.Quantity)
                        .FirstOrDefault();
                }
                return 0;
            });
            return total;
        }

        public OrderDetail CreateOrUpdateOrderDetail(string seatNo = "0", string staffId = null, string adminId = null)
        {
            var cart = GetCart(seatNo);
            if (!cart.Any()) return null;

            var existingOrder = _db.OrderDetails
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.SeatNo == int.Parse(seatNo) && o.Status == "Pending");

            if (existingOrder != null)
            {
                int index = existingOrder.OrderItems.Count + 1;
                foreach (var x in cart)
                {
                    var orderItem = new OrderItem
                    {
                        Id = $"{existingOrder.Id}-OI{index++}",
                        OrderDetailId = existingOrder.Id,
                        FoodId = int.Parse(x.Key),
                        Quantity = x.Value.Quantity,
                        SubTotal = int.TryParse(x.Key, out int foodId)
                            ? _db.Foods.Where(f => f.Id == foodId).Select(f => f.Price * x.Value.Quantity).FirstOrDefault()
                            : 0,
                        ExtraDetail = string.IsNullOrEmpty(x.Value.ExtraDetail) ? "N/A" : x.Value.ExtraDetail
                    };
                    _db.OrderItems.Add(orderItem);
                }

                existingOrder.Quantity += cart.Sum(x => x.Value.Quantity);
                existingOrder.TotalPrice += CalculateTotal(seatNo);

                _db.OrderDetails.Update(existingOrder);
                _db.SaveChanges();

                return existingOrder;
            }
            else
            {
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
                    AdminId = adminId
                };

                _db.OrderDetails.Add(orderDetail);
                _db.SaveChanges();

                int index = 1;
                foreach (var x in cart)
                {
                    var orderItem = new OrderItem
                    {
                        Id = $"{orderId}-OI{index++}",
                        OrderDetailId = orderId,
                        FoodId = int.Parse(x.Key),
                        Quantity = x.Value.Quantity,
                        SubTotal = int.TryParse(x.Key, out int foodId)
                            ? _db.Foods.Where(f => f.Id == foodId).Select(f => f.Price * x.Value.Quantity).FirstOrDefault()
                            : 0,
                        ExtraDetail = string.IsNullOrEmpty(x.Value.ExtraDetail) ? "N/A" : x.Value.ExtraDetail
                    };
                    _db.OrderItems.Add(orderItem);
                }

                _db.SaveChanges();
                return orderDetail;
            }
        }
    }
}