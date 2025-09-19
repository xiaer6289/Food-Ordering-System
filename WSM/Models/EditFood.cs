using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSM.Models;


public class EditFoodVM
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Food name is required.")]
    [StringLength(100, ErrorMessage = "Food name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.10, 9999.99, ErrorMessage = "Price must be between 0.10 and 9999.99.")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public string? CurrentPhoto { get; set; }

    public IFormFile? Photo { get; set; }

    [StringLength(6, ErrorMessage = "Category ID cannot exceed 6 characters.")]
    public string? CategoryId { get; set; }
}