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
    public DbSet<Payment> Payments { get; set; }
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

    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
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
    [Key, MaxLength(4)]
    public string Id { get; set; }

    [MaxLength(4)]
    public string SeatNo { get; set; }

    public int Quantity { get; set; }

    [MaxLength(20)]
    public string Status { get; set; }

    [Precision(5, 3)]

    public decimal TotalPrice { get; set; }

    public DateTime OrderDate { get; set; }

    //[ForeignKey("Staff")]
    public string StaffId { get; set; }

    public ICollection<Food> Foods { get; set; } = [];

    public Staff Staff { get; set; }

    public Payment Payment { get; set; }
}

public class Payment
{
    [Key, MaxLength(10)]
    public string Id { get; set; }

    [ForeignKey("OrderDetail")]
    public string OrderId { get; set; }

    [MaxLength(20)]
    public string PaymentMethod { get; set; }

    public DateTime Paymentdate { get; set; }

    public double AmountPaid { get; set; }

    public OrderDetail OrderDetail { get; set; }
}