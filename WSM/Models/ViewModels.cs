using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class IngredientVM
{
    [StringLength(4)]
    [RegularExpression(@"^[A-Z]{1}\d{3}", ErrorMessage = "Invalid {0}.")]
    [Remote("CheckId", "home", ErrorMessage = "Duplicated {0}/")]
    public string? Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; }
    public int? Quantity { get; set; }

    [Precision(5, 2)]
    public decimal? Kilogram { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }


}

public class PurchaseOrderVM
{
    public List<IngredientVM> Ingredients { get; set; } = new();
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

