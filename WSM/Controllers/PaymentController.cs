using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Globalization;
using WMS.Models;
using WSM.Controllers; 
using WSM.Helpers;
using WSM.Models;

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

        public IActionResult CreateCheckoutSession(string email)
        {
            var currency = "myr";
            var successUrl = Url.Action("Success", "Payment", null, Request.Scheme) + "?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme);
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var cart = _helper.GetCart();
            if (!cart.Any())
            {
                return BadRequest("Cart is empty.");
            }

            decimal totalAmount = cart.Sum(item => _db.Foods
                .Where(f => f.Id == item.Key)
                .Select(f => f.Price * item.Value)
                .FirstOrDefault());

            if (totalAmount <= 0)
            {
                return BadRequest("Invalid total amount.");
            }

            var orderId = GenerateOrderId();

            var options = new SessionCreateOptions
            {
                CustomerEmail = email,
                CustomerCreation = "always",
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = (long)(totalAmount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Restaurant Order",
                                Description = $"Order ID: {orderId}"
                            }
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId }
                }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        private string GenerateOrderId()
        {
            return "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        public async Task<IActionResult> Success(string session_id)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            if (session.PaymentStatus == "paid")
            {
                var orderId = session.Metadata["order_id"];
                if (string.IsNullOrEmpty(orderId))
                {
                    return BadRequest("Order ID not found in session metadata.");
                }

                // Get the cart data
                var cart = _helper.GetCart();
                if (!cart.Any())
                {
                    return BadRequest("Cart is empty during payment success.");
                }

                // Calculate total amount from session for verification
                decimal totalAmount = (decimal)session.AmountTotal / 100;

                // Create OrderDetail
                var orderDetail = new OrderDetail
                {
                    Id = orderId,
                    SeatNo = "N/A", // Adjust as needed
                    Quantity = cart.Sum(x => x.Value),
                    TotalPrice = totalAmount,
                    Status = "Completed",
                    OrderDate = DateTime.Now,
                    StaffId = "S001" // Adjust based on your logic (e.g., logged-in staff)
                };

                // Create OrderItems
                orderDetail.OrderItems = cart.Select(x => new OrderItem
                {
                    OrderDetailId = orderId,
                    FoodId = x.Key,
                    Quantity = x.Value,
                    SubTotal = _db.Foods.Where(f => f.Id == x.Key).Select(f => f.Price * x.Value).FirstOrDefault()
                }).ToList();

                _db.OrderDetails.Add(orderDetail);
                _db.SaveChanges(); // Save OrderDetail and OrderItems first

                // Create Payment
                var payment = new Payment
                {
                    PaymentId = "P" + orderId,
                    OrderDetailId = orderId, // Now this references an existing OrderDetail
                    PaymentMethod = "Card",
                    AmountPaid = (decimal)totalAmount,
                    Paymentdate = DateTime.Now,
                    StripeTransactionId = session.PaymentIntentId
                };
                _db.Payments.Add(payment);
                _db.SaveChanges(); // Save Payment

                // Clear the cart
                _helper.SetCart(new Dictionary<string, int>());
            }

            return View("OrderConfirmation");
        }

        public async Task<IActionResult> Cancel()
        {
            return View("OrderCancel");
        }
    }
}