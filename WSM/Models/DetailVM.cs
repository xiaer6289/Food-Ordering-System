
namespace WSM.Models;

public class DetailVM      //wrap multiple table
{
    public OrderDetail orderDetail { get; set; }
    public Company company { get; set; }
    public Staff staff { get; set; }
    public Admin admin { get; set; }
}
