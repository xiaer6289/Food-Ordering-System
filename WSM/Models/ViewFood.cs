using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class FoodVM
{
    public string? Id { get; set; }

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

    [Display(Name = "Upload Image")]
    public IFormFile Photo { get; set; }

    public string? CategoryName { get; set; }
}
