using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class FoodVM
{
    [StringLength(6)]
    public string Id { get; set; }

    [StringLength(100, ErrorMessage = "Food name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Range(0.10, 9999.99, ErrorMessage = "Price must be between 0.10 and 9999.99.")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL for the image.")]
    public string? Image { get; set; }

    [StringLength(6, ErrorMessage = "Category ID cannot exceed 6 characters.")]
    public string CategoryId { get; set; }
}

public class CategoryVM
{
    [StringLength(3)]
    public string Id { get; set; }

    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; }
}

public class FoodEditVM
{
    [StringLength(6)]
    public string Id { get; set; }

    [Required(ErrorMessage = "Food name is required.")]
    [StringLength(100, ErrorMessage = "Food name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.10, 9999.99, ErrorMessage = "Price must be between 0.10 and 9999.99.")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL for the image.")]
    public string? Image { get; set; }

    // CategoryId is optional for editing
    [StringLength(6, ErrorMessage = "Category ID cannot exceed 6 characters.")]
    public string? CategoryId { get; set; }
}