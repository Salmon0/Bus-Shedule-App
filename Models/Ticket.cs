using System.ComponentModel.DataAnnotations;

namespace BusSheduleApp.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BusRouteId { get; set; }

        [Display(Name = "Дата покупки")]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Номер места")]
        [Range(1, 100, ErrorMessage = "Номер места должен быть между 1 и 100")]
        public int SeatNumber { get; set; }

        [Required]
        [Display(Name = "Статус")]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public virtual User User { get; set; }
        public virtual BusRoute BusRoute { get; set; }

        public Ticket()
        {
            // Конструктор класса Ticket
        }

        ~Ticket()
        {
            // Деструктор класса Ticket
        }

        public bool Cancel()
        {
            if (Status == "Active")
            {
                Status = "Cancelled";
                BusRoute.AvailableSeats++;
                return true;
            }
            return false;
        }

        public bool MarkAsUsed()
        {
            if (Status == "Active")
            {
                Status = "Used";
                return true;
            }
            return false;
        }

        public bool CanBeCancelled()
        {
            return Status == "Active" &&
                   (BusRoute.DepartureTime - DateTime.UtcNow).TotalHours > 1;
        }
    }
}