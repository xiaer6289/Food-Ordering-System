using WSM.Models;

namespace WSM.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public OrderDetail OrderDetail { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Payment Payment { get; set; }
    }
}
