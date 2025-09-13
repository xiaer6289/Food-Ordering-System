using WSM.Models;

namespace WSM.Models;

public class DetailVM      //wrap multiple table
{
    public OrderDetail orderDetail { get; set; }
    public Company company { get; set; }
}
