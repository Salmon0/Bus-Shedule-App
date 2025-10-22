using BusSheduleApp.Models;
using BusSheduleApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DI и сервисы
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            var pendingMigrations = dbContext.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                dbContext.Database.Migrate();
                app.Logger.LogInformation("Applied {Count} pending migrations", pendingMigrations.Count());
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Error applying migrations");
        }
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseCors("AllowAll");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Routes}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Инициализация базы данных и тестовых маршрутов
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        // Добавление администратора, если его нет
        if (!context.Users.Any(u => u.Email == "admin@busapp.com"))
        {
            var admin = new User
            {
                UserName = "admin@busapp.com",
                Email = "admin@busapp.com",
                FirstName = "Admin",
                LastName = "Admin",
                PasswordHash = HashPassword("Admin!Test123"),
                Role = UserRole.Admin // или "Admin" если string
            };
            context.Users.Add(admin);
            context.SaveChanges();
        }

        if (!context.BusRoutes.Any())
        {
            context.BusRoutes.AddRange(
                new BusRoute
                {
                    DepartureCity = "Москва",
                    ArrivalCity = "Санкт-Петербург",
                    DepartureTime = DateTime.Now.AddDays(1),
                    ArrivalTime = DateTime.Now.AddDays(1).AddHours(8),
                    Price = 2500,
                    AvailableSeats = 30,
                    BusNumber = "МСП-001",
                    Status = "Ожидает"
                },
                new BusRoute
                {
                    DepartureCity = "Москва",
                    ArrivalCity = "Казань",
                    DepartureTime = DateTime.Now.AddDays(2),
                    ArrivalTime = DateTime.Now.AddDays(2).AddHours(12),
                    Price = 1800,
                    AvailableSeats = 25,
                    BusNumber = "МКЗ-002",
                    Status = "Ожидает"
                }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ошибка инициализации базы данных");
    }
}

// Метод хэширования пароля для seed-админа
static string HashPassword(string password)
{
    using (var sha = SHA256.Create())
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

app.Run();