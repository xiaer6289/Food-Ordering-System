using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace WSM.Helpers
{
    public class Helper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Helper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Dictionary<string, int> GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString("Cart") ?? "{}";
            return JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson) ?? new Dictionary<string, int>();
        }

        public void SetCart(Dictionary<string, int> cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString("Cart", JsonSerializer.Serialize(cart));
        }
    }
}