using Lesson10Areas.Data;
using Lesson10Areas.Models;
using Lesson10Areas.Services;
using Lesson10Areas.Services.Claims;
using Lesson10Areas.Services.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EcommerceDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration
            .GetConnectionString("default"))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking), ServiceLifetime.Transient);

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredUniqueChars = 5;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<EcommerceDbContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AgeLimit", policy => policy.Requirements.Add(new MinimumAgeRequirement(18)));
});

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.AccessDeniedPath = "/Home/AccessDenied";
//    options.Cookie.Name = "YourAppCookieName";
//    options.Cookie.HttpOnly = true;
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
//    options.LoginPath = "/Identity/Account/Login";
//    // ReturnUrlParameter requires 
//    //using Microsoft.AspNetCore.Authentication.Cookies;
//    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
//    options.SlidingExpiration = true;
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoint =>
{
    endpoint.MapAreaControllerRoute(
        name: "foradminarea",
        areaName: "Admin",
        pattern: "foradmin/{controller=Home}/{action=Index}"
        );
    //endpoint.MapControllerRoute(
    //    name: "foradmin",
    //    pattern: "{area}/{controller=Home}/{action=Index}"
    //    );
    endpoint.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

var container = app.Services.CreateScope();
var userManager = container.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
var roleManager = container.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
if (!await roleManager.RoleExistsAsync("Admin"))
{
    var result = await roleManager.CreateAsync(new IdentityRole("Admin"));
    if (!result.Succeeded) throw new Exception(result.Errors.First().Description);
}

var user = await userManager.FindByEmailAsync("admin@admin.com");
if (user is null)
{
    user = new AppUser
    {
        UserName = "admin@admin.com",
        Email = "admin@admin.com",
        FullName = "Admin",
        Year = 2023,
        EmailConfirmed = true
    };
    var result = await userManager.CreateAsync(user, "Admin12!");
    if (!result.Succeeded) throw new Exception(result.Errors.First().Description);
    result = await userManager.AddToRoleAsync(user, "Admin");
}

app.Run();
