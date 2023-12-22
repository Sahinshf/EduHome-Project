using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});


// Service

// Adding İdentity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password`s requirements
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    // Unique Email
    options.User.RequireUniqueEmail = true;

    options.Lockout.MaxFailedAccessAttempts = 3; // Səhv cəhdlərin sayı
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30);

}).AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();// Default role istifadə edirik

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(30);
});

Constants.mail = builder.Configuration["MailSettings:Mail"];
Constants.password = builder.Configuration["MailSettings:Password"];
Constants.host = builder.Configuration["MailSettings:Host"];
Constants.port = int.Parse(builder.Configuration["MailSettings:Port"]);

builder.Services.AddControllersWithViews();

var app = builder.Build();


// Middleware

app.UseStaticFiles();

app.UseSession();

app.UseAuthentication(); // Login`dən istifadə etdiyimizi bildiririk
app.UseAuthorization(); // Sistemə daxil olduqdan sonra role`lardan istifadə edəcəyimizi bildiririk. Access`ləri idarə edirik

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
    
    );

app.MapControllerRoute(

    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{id?}"

    );


app.Run();
