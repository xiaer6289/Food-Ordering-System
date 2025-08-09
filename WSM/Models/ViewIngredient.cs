using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class IngredientVM
{
    [StringLength(4)]
    [RegularExpression(@"^I\d{3}", ErrorMessage = "Invalid {0}.")]
    public string? Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    [Range(1,99999)]
    public int? Quantity { get; set; }

    [Range(0.01, 999999.99)]
    [Precision(5, 2)]
    public decimal? Kilogram { get; set; }
    public decimal Price { get; set; }

    [Precision(8, 2)]
    public decimal TotalPrice { get; set; }
}



public class LoginVM
{
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}

