using Microsoft.EntityFrameworkCore;
using Online_Order_for_digital_printing_of_Photos.Middleware;
using Online_Order_for_digital_printing_of_Photos.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register MyImageDbContext with SQL Server
builder.Services.AddDbContext<MyImageDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add session and distributed memory cache services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;                // Make session cookie HTTP-only
    options.Cookie.IsEssential = true;             // Mark the cookie as essential
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Enable session middleware
app.UseRouting();
app.UseMiddleware<RoleBasedAccessMiddleware>(); // Add custom middleware
app.UseAuthorization();

// Configure default routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
