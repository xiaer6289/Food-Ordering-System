using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WSM.Models;

namespace WSM.Models;

public class DB : DbContext
{
    public DB(DbContextOptions<DB> options) : base(options) { }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
}

public class Company
{
    [Key]
    public int CompanyId { get; set; }   // Primary Key
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string? LogoPath { get; set; }
}

public class Food
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(6)]
    public string Id { get; set; }  

    [Required(ErrorMessage = "Food name is required.")]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(0.10, 9999.99, ErrorMessage = "Price must be between 0.10 and 9999.99.")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(255)]
    [Url(ErrorMessage = "Please enter a valid URL for the image.")]
    public string? Image { get; set; }   

    [Required(ErrorMessage = "Category is required.")]
    [ForeignKey("Category")]
    [MaxLength(6)]
    public string CategoryId { get; set; }

    public Category Category { get; set; }
}

public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] 
    [MaxLength(6)]
    public string Id { get; set; }  // e.g., "C0001"

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100)]
    public string Name { get; set; }

    // Navigation
    public ICollection<Food> Foods { get; set; }
}


public class Admin
{
    [Key, MaxLength(6)]
    public string Id { get; set; }
    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [MaxLength(20)]
    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,20}$",
        ErrorMessage = "Password must be 8 to 20 characters long, with at least one uppercase letter, one lowercase letter, one digit, and one special character (!@#$%^&*).")]
    public string Password { get; set; }

    [MaxLength(50)]
    [Required]
    public string Name { get; set; }

    [MaxLength(15)]
    [Required]
    [RegularExpression(@"^01[0-9]{8,13}$", ErrorMessage = "Phone number must start with '01' and be 10 to 15 digits long.")]
    public string PhoneNo { get; set; }

    public ICollection<Staff> Staffs { get; set; } = new List<Staff>();
}

public class Staff
{
    [Key, MaxLength(4)]
    public string Id { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [MaxLength(20)]
    [Required]
    public string Password { get; set; }

    [MaxLength(50)]
    [Required]
    public string Name { get; set; }

    [Required]
    public double Salary { get; set; }

    [MaxLength(15)]
    [Required]
    public string PhoneNo { get; set; }

    public string AdminId { get; set; }

    [ForeignKey("AdminId")]
    public Admin Admin { get; set; }
}

public class OrderDetail
{
    [Key, MaxLength(20)]
    public string Id { get; set; }

    [MaxLength(4)]
    public string SeatNo { get; set; }

    [Required]
    public int Quantity { get; set; }

    [MaxLength(20)]
    [Required]
    public string Status { get; set; }

    [Precision(10, 2)]
    [Required]
    public decimal TotalPrice { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [ForeignKey("Staff")]
    public string StaffId { get; set; }
    public Staff Staff { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public Payment Payment { get; set; }
}

public class OrderItem
{
    [Key, MaxLength(30)]
    public string Id { get; set; }

    [MaxLength(20)]
    [Required]
    public string OrderDetailId { get; set; }
    public OrderDetail OrderDetail { get; set; }

    [MaxLength(6)]
    [Required]
    public string FoodId { get; set; }
    public Food Food { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Precision(6, 2)]
    [Required]
    public decimal SubTotal { get; set; }

    [MaxLength(200)]
    public string ExtraDetail { get; set; }
}

public class Payment
{
    [Key, MaxLength(20)]
    public string Id { get; set; }

    [ForeignKey("OrderDetail")]
    [Required]
    public string OrderDetailId { get; set; }

    [MaxLength(20)]
    [Required]
    public string PaymentMethod { get; set; }

    [Precision(10, 2)]
    [Required]
    public decimal TotalPrice { get; set; }

    [Precision(10, 2)]
    [Required]
    public decimal AmountPaid { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
    [Required]
    public DateTime Paymentdate { get; set; }

    public string StripeTransactionId { get; set; } // save the stripe id

    [ForeignKey(nameof(OrderDetailId))]
    public OrderDetail OrderDetail { get; set; }
}

public class Ingredient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [MaxLength(50)]
    [Required]
    public string Name { get; set; }
    public int? Quantity { get; set; }
    [Precision(5, 3)]
    public decimal? Kilogram { get; set; }
    [Precision(5, 2)]
    [Required]
    public decimal Price { get; set; }
    [Precision(6, 2)]
    [Required]
    public decimal TotalPrice { get; set; }

    public ICollection<Food> Foods { get; set; } = new List<Food>();
}
