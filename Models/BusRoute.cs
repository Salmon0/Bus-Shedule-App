using BusSheduleApp.Models;
using System.ComponentModel.DataAnnotations;

namespace BusSheduleApp.Models
{
    public class BusRoute
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Город отправления обязателен")]
        [Display(Name = "Город отправления")]
        [StringLength(100, ErrorMessage = "Название города не может превышать 100 символов")]
        public string DepartureCity { get; set; }

        [Required(ErrorMessage = "Город прибытия обязателен")]
        [Display(Name = "Город прибытия")]
        [StringLength(100, ErrorMessage = "Название города не может превышать 100 символов")]
        public string ArrivalCity { get; set; }

        [Required(ErrorMessage = "Время отправления обязательно")]
        [Display(Name = "Время отправления")]
        [DataType(DataType.DateTime)]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Время прибытия обязательно")]
        [Display(Name = "Время прибытия")]
        [DataType(DataType.DateTime)]
        public DateTime ArrivalTime { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Display(Name = "Цена")]
        [Range(0, 100000, ErrorMessage = "Цена должна быть между 0 и 100000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Количество мест обязательно")]
        [Display(Name = "Свободные места")]
        [Range(0, 100, ErrorMessage = "Количество мест должно быть между 0 и 100")]
        public int AvailableSeats { get; set; }

        [Required(ErrorMessage = "Номер автобуса обязателен")]
        [Display(Name = "Номер автобуса")]
        [StringLength(20, ErrorMessage = "Номер автобуса не может превышать 20 символов")]
        public string BusNumber { get; set; }

        [Required]
        [Display(Name = "Статус")]
        [StringLength(20)]
        public string Status { get; set; } = "Ожидает";

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public TimeSpan GetDuration()
        {
            return ArrivalTime - DepartureTime;
        }

        public bool HasAvailableSeats(int requestedSeats = 1)
        {
            return AvailableSeats >= requestedSeats;
        }

        public bool ReserveSeats(int seatsToReserve)
        {
            if (HasAvailableSeats(seatsToReserve))
            {
                AvailableSeats -= seatsToReserve;
                return true;
            }
            return false;
        }

        public void UpdateStatusByTime(DateTime now)
        {
            if (Status == "Ожидает" && now >= DepartureTime)
                Status = "Отправился";
            if (Status == "Отправился" && now >= ArrivalTime)
                Status = "Завершен";
        }

        public BusRoute()
        {
            // Конструктор класса BusRoute
        }

        ~BusRoute()
        {
            // Деструктор класса BusRoute
        }
    }
}