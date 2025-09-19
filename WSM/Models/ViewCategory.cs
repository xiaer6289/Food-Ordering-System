using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class CategoryVM
{
    [StringLength(3)]
    public string Id { get; set; }

    [StringLength(30, ErrorMessage = "Category name cannot exceed 30 characters.")]
    [Required(ErrorMessage = "Category name is required")]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Category name can only contain letters and spaces")]
    public string Name { get; set; }
}
