using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using WSM.Helpers;
using WSM.Models;
using WSM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace WSM.Controllers
{
    public class PaymentController : Controller
    {
        private readonly StripeSettings _stripeSettings;
        private readonly Helper _helper;
        private readonly DB _db;

        public PaymentController(IOptions<StripeSettings> stripeSettings, Helper helper, DB db)
        {
            _stripeSettings = stripeSettings.Value;
            _helper = helper;
            _db = db;
        }

        [HttpPost]
        [HttpPost]
        public IActionResult CreateCheckoutSession(string seatNo)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            string role = HttpContext.Session.GetString("Role");
            string staffAdminId = HttpContext.Session.GetString("StaffAdminId");

            OrderDetail orderDetail = null;
            if (role == "Staff")
            {
                orderDetail = _helper.CreateOrderDetail(seatNo, staffId: staffAdminId);
            }
            else if (role == "Admin")
            {
                orderDetail = _helper.CreateOrderDetail(seatNo, adminId: staffAdminId);
            }

            if (orderDetail == null)
                return BadRequest("Cart is empty.");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "myr",
                            UnitAmount = (long)(orderDetail.TotalPrice * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Restaurant Order",
                                Description = $"Order ID: {orderDetail.Id}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Payment/Success?seatNo={seatNo}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Payment/Cancel",
                Metadata = new Dictionary<string, string> { { "order_id", orderDetail.Id } },
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }


        public async Task<IActionResult> Success(string seatNo, string session_id)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            if (session.PaymentStatus != "paid")
                return View("OrderCancel");

            string orderId = session.Metadata["order_id"];

            // Save payment info
            var payment = new Payment
            {
                Id = "P" + orderId.Substring(3),
                OrderDetailId = orderId,
                PaymentMethod = "Card",
                AmountPaid = (decimal)session.AmountTotal / 100,
                Paymentdate = DateTime.Now,
                StripeTransactionId = session.PaymentIntentId
            };
            _db.Payments.Add(payment);
            _db.SaveChanges();

            var orderDetail = _db.OrderDetails
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Food)
                .FirstOrDefault(o => o.Id == orderId);


            var orderItems = _db.OrderItems
                                .Include(i => i.Food)
                                .Where(i => i.OrderDetailId == orderId)
                                .ToList();

      

            // Pass everything to the view
            var model = new OrderConfirmationViewModel
            {
                OrderDetail = orderDetail,
                OrderItems = orderDetail.OrderItems.ToList(),
                Payment = payment
            };

            // clear cart
            _helper.SetCart(seatNo, new Dictionary<string, Helper.CartItem>());
            return View("OrderConfirmation", model);

        }

        // Payment cancel page
        public IActionResult Cancel()
        {
            return View("OrderCancel");
        }
    }
}
