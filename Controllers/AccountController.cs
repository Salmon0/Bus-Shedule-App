using Microsoft.AspNetCore.Mvc;
using BusSheduleApp.Models;
using BusSheduleApp.Data;
using System.Security.Cryptography;
using System.Text;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Пользователь с таким Email уже существует.");
                return View(model);
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Patronymic = model.Patronymic,
                PhoneNumber = model.PhoneNumber,
                BirthDate = model.BirthDate,
                PasswordHash = HashPassword(model.Password)
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            _logger.LogInformation("User registered: {Email}", user.Email);
            // Вход после регистрации
            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Index", "Routes");
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null && VerifyPassword(model.Password, user.PasswordHash))
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                _logger.LogInformation("User logged in: {Email}", user.Email);
                return RedirectToAction("Index", "Routes");
            }
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
        }
        return View(model);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("UserId");
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Manage()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }
        var user = _context.Users.Find(userId.Value);
        if (user == null)
        {
            return RedirectToAction("Login");
        }
        return View(user);
    }

    // Хэширование пароля
    private string HashPassword(string password)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    // Проверка пароля
    private bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}

// =====================
// ДАННЫЕ ДЛЯ АДМИНА:
// Логин: admin@busapp.com, Пароль: Admin!Test123
// =====================

