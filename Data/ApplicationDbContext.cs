using Microsoft.EntityFrameworkCore;
using BusSheduleApp.Models;

namespace BusSheduleApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BusRoute> BusRoutes { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Конфигурация связей
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.BusRoute)
                .WithMany(r => r.Tickets)
                .HasForeignKey(t => t.BusRouteId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        // Методы для работы с пользователями
        public User FindUserById(int id) => Users.FirstOrDefault(u => u.Id == id);
        public User FindUserByUserName(string userName) => Users.FirstOrDefault(u => u.UserName == userName);
        public User FindUserByEmail(string email) => Users.FirstOrDefault(u => u.Email == email);
        public IQueryable<Ticket> GetUserTickets(int userId) => Tickets.Where(t => t.UserId == userId);

        // Методы для работы с маршрутами и билетами
        public BusRoute GetRouteById(int id) => BusRoutes.FirstOrDefault(r => r.Id == id);
        public Ticket GetTicketById(int id) => Tickets.FirstOrDefault(t => t.Id == id);
    }
}
