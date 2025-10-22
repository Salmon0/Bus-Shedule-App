using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusSheduleApp.Models;
using BusSheduleApp.Data;

[AllowAnonymous]
public class RoutesController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoutesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string from = null, string to = null, DateTime? date = null)
    {
        var query = _context.BusRoutes.AsQueryable();

        if (!string.IsNullOrEmpty(from))
        {
            query = query.Where(r => r.DepartureCity.Contains(from));
        }

        if (!string.IsNullOrEmpty(to))
        {
            query = query.Where(r => r.ArrivalCity.Contains(to));
        }

        if (date.HasValue)
        {
            query = query.Where(r => r.DepartureTime.Date == date.Value.Date);
        }

        var routes = await query.OrderBy(r => r.DepartureTime).ToListAsync();

        // Обновляем статусы и удаляем завершённые маршруты
        var now = DateTime.Now;
        var changed = false;
        foreach (var route in routes.ToList())
        {
            var oldStatus = route.Status;
            route.UpdateStatusByTime(now);
            if (route.Status != oldStatus)
                changed = true;
            if (route.Status == "Завершен")
            {
                _context.BusRoutes.Remove(route);
                routes.Remove(route);
                changed = true;
            }
        }
        if (changed)
            await _context.SaveChangesAsync();

        ViewBag.SearchFrom = from;
        ViewBag.SearchTo = to;
        ViewBag.SearchDate = date;

        return View(routes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var route = await _context.BusRoutes
            .Include(r => r.Tickets)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (route == null)
        {
            return NotFound();
        }

        // Получаем занятые места для отображения
        var takenSeats = route.Tickets
            .Where(t => t.Status != "Cancelled")
            .Select(t => t.SeatNumber)
            .ToList();

        ViewBag.TakenSeats = takenSeats;
        ViewBag.AvailableSeats = Enumerable.Range(1, route.AvailableSeats + takenSeats.Count)
            .Where(s => !takenSeats.Contains(s))
            .ToList();

        return View(route);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Account");
        var user = _context.Users.Find(userId.Value);
        if (user == null || user.Role != UserRole.Admin)
            return Forbid();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BusRoute model)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Account");
        var user = _context.Users.Find(userId.Value);
        if (user == null || user.Role != UserRole.Admin)
            return Forbid();
        if (ModelState.IsValid)
        {
            _context.BusRoutes.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Account");
        var user = _context.Users.Find(userId.Value);
        if (user == null || user.Role != UserRole.Admin)
            return Forbid();
        var route = await _context.BusRoutes.FindAsync(id);
        if (route == null)
            return NotFound();
        _context.BusRoutes.Remove(route);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}