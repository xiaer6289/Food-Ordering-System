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
        public IActionResult CreateCheckoutSession(string seatNo)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var pendingOrders = _db.OrderDetails
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .Where(o => o.SeatNo == int.Parse(seatNo) && o.Status != "Paid")
                .ToList();

            if (!pendingOrders.Any())
                return BadRequest("No pending orders to pay.");

            var subtotal = pendingOrders.Sum(o => o.TotalPrice);
            var tax = subtotal * 0.06m;
            var serviceCharge = subtotal * 0.1m;
            var totalAmountWithCharges = subtotal + tax + serviceCharge;

            HttpContext.Session.SetString("Subtotal", subtotal.ToString());
            HttpContext.Session.SetString("Tax", tax.ToString());
            HttpContext.Session.SetString("ServiceCharge", serviceCharge.ToString());
            HttpContext.Session.SetString("TotalAmount", totalAmountWithCharges.ToString());

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
                            Description = $"Seat No: {seatNo} - {pendingOrders.Count} orders\n"
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

            HttpContext.Session.SetString("OrdersToPay", string.Join(",", pendingOrders.Select(o => o.Id)));

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success(string seatNo, string session_id)
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
            var service = new SessionService();
            var session = await service.GetAsync(session_id);

            if (session.PaymentStatus != "paid")
                return View("OrderCancel");

            var orderIdsStr = HttpContext.Session.GetString("OrdersToPay") ?? "";
            var orderIds = orderIdsStr.Split(",", StringSplitOptions.RemoveEmptyEntries);

            var orders = _db.OrderDetails
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .Where(o => orderIds.Contains(o.Id))
                .ToList();

            if (!orders.Any())
                return BadRequest("No matching orders found.");

            var totalAmount = orders.Sum(o => o.TotalPrice);

            var subtotal = decimal.Parse(HttpContext.Session.GetString("Subtotal") ?? "0");
            var tax = decimal.Parse(HttpContext.Session.GetString("Tax") ?? "0");
            var serviceCharge = decimal.Parse(HttpContext.Session.GetString("ServiceCharge") ?? "0");
            var totalAmountWithCharges = decimal.Parse(HttpContext.Session.GetString("TotalAmount") ?? "0");

            var payment = new Payment
            {
                Id = "P" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrderDetailId = orders.First().Id,
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


            foreach (var order in orders)
            {
                order.Status = "Paid";
                _db.OrderDetails.Update(order);
            }

            var seat = _db.Seats.FirstOrDefault(s => s.SeatNo == int.Parse(seatNo));
            if (seat != null)
            {
                seat.Status = "Available";
                _db.Seats.Update(seat);
            }

            _db.SaveChanges();

            var model = new OrderConfirmationViewModel
            {
                OrderDetail = orders.First(),
                OrderItems = orders.SelectMany(o => o.OrderItems).ToList(),
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
