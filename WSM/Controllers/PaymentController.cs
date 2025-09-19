using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using WSM.Helpers;
using WSM.Models;
using WSM.ViewModels;

namespace WSM.Controllers
{
    [Authorize]
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

            var order = _db.OrderDetails
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .FirstOrDefault(o => o.SeatNo == int.Parse(seatNo) && o.Status == "Pending");

            if (order == null)
                return BadRequest("No pending order to pay.");

            var subtotal = order.TotalPrice;
            var tax = subtotal * 0.06m;
            var serviceCharge = subtotal * 0.1m;
            var totalAmountWithCharges = subtotal + tax + serviceCharge;

            HttpContext.Session.SetString("Subtotal", subtotal.ToString());
            HttpContext.Session.SetString("Tax", tax.ToString());
            HttpContext.Session.SetString("ServiceCharge", serviceCharge.ToString());
            HttpContext.Session.SetString("TotalAmount", totalAmountWithCharges.ToString());
            HttpContext.Session.SetString("OrderIdToPay", order.Id);

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
                            UnitAmount = (long)(totalAmountWithCharges * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Restaurant Order",
                                Description = $"Seat No: {seatNo}, Order ID: {order.Id}"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Payment/Success?seatNo={seatNo}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Payment/Cancel"
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

            var orderId = HttpContext.Session.GetString("OrderIdToPay");
            if (string.IsNullOrEmpty(orderId))
                return BadRequest("No order found in session.");

            var order = _db.OrderDetails
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return BadRequest("Order not found.");

            var subtotal = decimal.Parse(HttpContext.Session.GetString("Subtotal") ?? "0");
            var tax = decimal.Parse(HttpContext.Session.GetString("Tax") ?? "0");
            var serviceCharge = decimal.Parse(HttpContext.Session.GetString("ServiceCharge") ?? "0");
            var totalAmountWithCharges = decimal.Parse(HttpContext.Session.GetString("TotalAmount") ?? "0");

            var payment = new Payment
            {
                Id = "P" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderDetailId = order.Id,
                PaymentMethod = "Card",
                Subtotal = subtotal,
                Tax = tax,
                ServiceCharge = serviceCharge,
                TotalPrice = totalAmountWithCharges,
                AmountPaid = (decimal)session.AmountTotal / 100,
                Paymentdate = DateTime.Now,
                StripeTransactionId = session.PaymentIntentId
            };
            _db.Payments.Add(payment);

            order.Status = "Paid";
            _db.OrderDetails.Update(order);

            var seat = _db.Seats.FirstOrDefault(s => s.SeatNo == int.Parse(seatNo));
            if (seat != null)
            {
                seat.Status = "Available";
                _db.Seats.Update(seat);
            }

            _db.SaveChanges();

            var model = new OrderConfirmationViewModel
            {
                OrderDetail = order,
                OrderItems = order.OrderItems.ToList(),
                Payment = payment
            };

            return View("OrderConfirmation", model);
        }


        // Payment cancel page
        public IActionResult Cancel()
        {
            return View("OrderCancel");
        }
    }
}
