using System.ComponentModel.DataAnnotations;

namespace WMS.Models
{
    public class Menu
    {

        public int Id  { get; set; }
        public decimal Value { get; set; }

        [Required]
        public string? Description { get; set; }

    }
}
