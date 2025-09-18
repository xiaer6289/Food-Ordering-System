using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class ViewAdmin
{
    [StringLength(6)]
    public string Id { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(15)]
    [RegularExpression(@"^01[0-9]{8,13}$", ErrorMessage = "Phone number must start with '01' and be 10 to 15 digits long.")]
    public string PhoneNo { get; set; }
}
