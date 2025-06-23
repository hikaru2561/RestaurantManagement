using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;
using RestaurantManagement.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// ✅ Thêm dòng này để hỗ trợ View trong Area
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.AreaViewLocationFormats.Clear();
    options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
});

// Add authentication using cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
       // var uniqueSuffix = DateTime.Now.Ticks;
        //options.Cookie.Name = $".AspNetCore.CustomAuthCookie_{uniqueSuffix}";

        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/Login/Denied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure HTTP request pipeline
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Tạo tài khoản admin mặc định nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate(); // chạy migration nếu cần

    if (!db.Accounts.Any(a => a.Username == "admin"))
    {
        db.Accounts.Add(new Account
        {
            Username = "admin",
            Password = HashHelper.ComputeSha256Hash("admin123"),
            Role = "Admin"
        });
        db.SaveChanges();
    }
}

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"); // PHẢI có

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
