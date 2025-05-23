using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Task_Management_System.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add MVC controllers with views support
builder.Services.AddControllersWithViews();

// Configure EF Core DbContext with SQL Server and ignore PendingModelChangesWarning
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("myconn"));
    
});


// Configure Identity with custom User and integer roles
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure application cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Manager}/{action=Index}/{id?}");

app.Run();
