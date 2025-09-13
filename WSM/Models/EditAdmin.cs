using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class EditAdminViewModel
{
    [Required]
    [StringLength(6)]
    public string Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(15)]
    [RegularExpression(@"^01[0-9]{8,13}$", ErrorMessage = "Phone number must start with '01' and be 10 to 15 digits long.")]
    public string PhoneNo { get; set; }

    [StringLength(20, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,20}$",
        ErrorMessage = "Password must be 8-20 characters with uppercase, lowercase, digit, and special character.")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "New password and confirmation do not match.")]
    public string? ConfirmPassword { get; set; }
}