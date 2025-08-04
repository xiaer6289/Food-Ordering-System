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
    public string Name { get; set; }

    [Precision(5,3)]
    public decimal Price { get; set; }

    [MaxLength(100)]
    public string Description { get; set; }

    [MaxLength(100)]
    public string Image { get; set; }

    //[ForeignKey("Category")]
    public string CategoryId { get; set; }
    
    public Category Category { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<Ingredient> Ingredients { get; set; } = [];
}

public class Category
{
    [Key, MaxLength(2)]
    public string Id { get; set; }

    public ICollection<Food> Foods { get; set; } = []; // navigation
}

public class Admin
{
    [Key, MaxLength(4)]
    public string Id { get; set; }

    [MaxLength(20)]
    public string Password { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(15)]
    public string PhoneNo { get; set; }

    public ICollection<Staff> Staffs { get; set; } = [];
}

public class Staff //use salary to differentiate waiter,manager...
{
    [Key, MaxLength(4)]
    public string Id { get; set; }

    [MaxLength(20)]
    public string Password { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }

    public double Salary { get; set; }

    [MaxLength(15)]
    public string PhoneNo { get; set; }


    //[ForeignKey("Admin")]
    public string AdminId { get; set; }

    public Admin Admin { get; set; } 
}

public class OrderDetail
{
    [Key, MaxLength(20)]
    public string Id { get; set; }

    [MaxLength(4)]
    public string SeatNo { get; set; }

    //Total Quantity
    public int Quantity { get; set; }

    [MaxLength(20)]
    public string Status { get; set; }

    [Precision(10, 2)]
    public decimal TotalPrice { get; set; }

    public DateTime OrderDate { get; set; }

    //[ForeignKey("Staff")]
    public string StaffId { get; set; }
    public Staff Staff { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = [];

    public Payment Payment { get; set; }
}

public class OrderItem
{
    [Key]
    public string Id { get; set; }

    [MaxLength(20)]
    public string OrderDetailId { get; set; }
    public OrderDetail OrderDetail { get; set; }

    [MaxLength(4)]
    public string FoodId { get; set; }
    public Food Food { get; set; }

    public int Quantity { get; set; }

    [Precision(6, 2)]
    public decimal SubTotal { get; set; }
}


public class Payment
{
    [Key, MaxLength(20)]
    public string PaymentId { get; set; }

    [ForeignKey("OrderDetail")]
    public string OrderDetailId { get; set; } 

    [MaxLength(20)]
    public string PaymentMethod { get; set; }

    [Precision(10, 2)]
    public decimal TotalPrice { get; set; }

    [Precision(10, 2)]
    public decimal AmountPaid { get; set; }

    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")] 
    public DateTime Paymentdate { get; set; }

    public string StripeTransactionId { get; set; } // save the stripe id

    [ForeignKey(nameof(OrderDetailId))]
    public OrderDetail OrderDetail { get; set; }
}

public class Ingredient
{
    [Key, MaxLength(4)]
    public string Id { get; set; }
    [MaxLength(50)]
    public string Name { get; set; }
    public int? Quantity { get; set; }
    [Precision(5, 3)]
    public decimal? Kilogram { get; set; }
    [Precision(5, 2)]
    public decimal Price { get; set; }
    [Precision(6,2)]
    public decimal TotalPrice { get; set; }

    public ICollection<Food> Foods { get; set; } = [];


}