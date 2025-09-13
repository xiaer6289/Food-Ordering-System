using System.ComponentModel.DataAnnotations;

namespace WSM.Models;

public class CategoryVM
{
    [StringLength(3)]
    public string Id { get; set; }

    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
    public string Name { get; set; }
}
