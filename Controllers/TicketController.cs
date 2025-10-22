using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusSheduleApp.Models;
using BusSheduleApp.Data;
using Microsoft.Extensions.Logging;

public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ApplicationDbContext context, ILogger<TicketsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private User GetCurrentUser()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return null;
        return _context.Users.Find(userId.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Purchase(int routeId, int seatNumber)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        _logger.LogInformation($"[Purchase] userId from session: {userId}");
        var route = await _context.BusRoutes.FindAsync(routeId);
        if (route == null)
        {
            return NotFound("Маршрут не найден");
        }
        if (!route.HasAvailableSeats())
        {
            return BadRequest("Нет свободных мест на выбранный маршрут");
        }
        var user = GetCurrentUser();
        if (user == null)
        {
            _logger.LogWarning("[Purchase] Пользователь не найден по userId");
            return RedirectToAction("Login", "Account");
        }
        if (await _context.Tickets.AnyAsync(t => t.BusRouteId == routeId && t.SeatNumber == seatNumber))
        {
            return BadRequest("Выбранное место уже занято");
        }
        var ticket = new Ticket
        {
            UserId = user.Id,
            BusRouteId = routeId,
            SeatNumber = seatNumber,
            PurchaseDate = DateTime.UtcNow,
            Status = "Active"
        };
        route.ReserveSeats(1);
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = ticket.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        _logger.LogInformation($"[Details] userId from session: {userId}");
        var user = GetCurrentUser();
        if (user == null) return RedirectToAction("Login", "Account");
        var ticket = await _context.Tickets
            .Include(t => t.BusRoute)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
        if (ticket == null)
        {
            return NotFound();
        }
        return View(ticket);
    }

    public async Task<IActionResult> MyTickets()
    {
        var user = GetCurrentUser();
        if (user == null) return Unauthorized();
        var tickets = await _context.Tickets
            .Include(t => t.BusRoute)
            .Where(t => t.UserId == user.Id)
            .OrderByDescending(t => t.PurchaseDate)
            .ToListAsync();

        return View(tickets);
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var user = GetCurrentUser();
        if (user == null) return Unauthorized();
        var ticket = await _context.Tickets
            .Include(t => t.BusRoute)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);

        if (ticket == null)
        {
            return NotFound();
        }

        if (!ticket.CanBeCancelled())
        {
            return BadRequest("Этот билет нельзя отменить");
        }

        if (ticket.Cancel())
        {
            await _context.SaveChangesAsync();
            return RedirectToAction("MyTickets");
        }

        return BadRequest("Не удалось отменить билет");
    }
}