using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WSM.Models;

public class DB : DbContext
{
    public DB(DbContextOptions<DB> options) : base(options) { }

    public DbSet<Food> Foods { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }


}

public class Food
{
    [Key, MaxLength(4)]
    public string Id { get; set; }

    [MaxLength(50)]
    [Required]
    public string Name { get; set; }

    [Precision(5, 3)]
    [Required]
    public decimal Price { get; set; }

    [MaxLength(100)]
    public string Description { get; set; }

    [MaxLength(100)]
    public string Image { get; set; }

    [ForeignKey(nameof(Category))] // Use nameof to avoid ambiguity
    [Required(ErrorMessage = "Please select a category.")]
    public string CategoryId { get; set; }

    public Category Category { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<Ingredient> Ingredients { get; set; } = [];
}

public class Category
{
    [Key, MaxLength(2)]
    public string Id { get; set; }

    [MaxLength(50)]
    [Required]
    public string Name { get; set; }

    public ICollection<Food> Foods { get; set; } = []; // navigation
}

public class Admin
{
    [Key, MaxLength(6)]
    public string AdminId { get; set; }

    [MaxLength(20)]
    [Required]
    public string Password { get; set; }

    [MaxLength(50)]
    [Required]
    public string Name { get; set; }

    [MaxLength(15)]
    [Required]
    public string PhoneNo { get; set; }

    public ICollection<Staff> Staffs { get; set; } = [];
}

public class Staff
{
    [Key, MaxLength(4)]
    public string Id { get; set; }

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

    public ICollection<OrderItem> OrderItems { get; set; } = [];

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

    [MaxLength(4)]
    [Required]
    public string FoodId { get; set; }
    public Food Food { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Precision(6, 2)]
    [Required]
    public decimal SubTotal { get; set; }
}

public class Payment
{
    [Key, MaxLength(20)]
    public string PaymentId { get; set; }

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

    public ICollection<Food> Foods { get; set; } = [];
}