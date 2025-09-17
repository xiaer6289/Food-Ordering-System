using System.ComponentModel.DataAnnotations;

namespace WMS.Models
{
    public class EditStaffViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public double Salary { get; set; }
        public string? NewPassword { get; set; }
    }
}
