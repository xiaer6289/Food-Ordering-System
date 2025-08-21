using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WSM.Models;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllersWithViews();
//Stripe 
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

//Session - Cart 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DB>();
builder.Services.AddScoped<WSM.Helpers.Helper>();

builder.Services.AddScoped<IPasswordHasher<string>, PasswordHasher<string>>();


builder.Services.AddDbContext<DB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddSqlServer<DB>($@"
    Data Source=(LocalDB)\MSSQLLocalDB;
    AttachDbFilename={builder.Environment.ContentRootPath}\DB.mdf;
    Integrated Security=True;
");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authorization}/{action=Login}/{id?}");
//The default page when run the program


app.Run();
