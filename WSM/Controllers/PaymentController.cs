using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using WMS.Models;
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

        // Create Stripe Checkout session
        [HttpPost]
        public IActionResult CreateCheckoutSession(string seatNo)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var orderDetail = _helper.CreateOrderDetail(seatNo, "S001");
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
                SuccessUrl = Url.Action("Success", "Payment", null, Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme),
                Metadata = new Dictionary<string, string> { { "order_id", orderDetail.Id } },
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        // Payment success callback
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

            // Get order details
            var orderDetail = _db.OrderDetails
                                 .FirstOrDefault(o => o.Id == orderId);

            // Get list of foods in this order (Include Food for FoodName)
            var orderItems = _db.OrderItems
                                .Include(i => i.Food)
                                .Where(i => i.OrderDetailId == orderId)
                                .ToList();

            // clear cart
            _helper.SetCart(seatNo, new Dictionary<string, int>());

            // Pass everything to the view
            var model = new OrderConfirmationViewModel
            {
                OrderDetail = orderDetail,
                OrderItems = orderItems,
                Payment = payment
            };

            _helper.SetCart(seatNo, new Dictionary<string, int>());

            return View("OrderConfirmation", model);

        }

        // Payment cancel page
        public IActionResult Cancel()
        {
            return View("OrderCancel");
        }
    }
}
