using System.ComponentModel.DataAnnotations;

namespace BusSheduleApp.Models
{
    public class BaseEntity
    {
        public int Id { get; set; }
    }

    public class Person : BaseEntity
    {
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не может превышать 50 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        [StringLength(50, ErrorMessage = "Фамилия не может превышать 50 символов")]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        [StringLength(50, ErrorMessage = "Отчество не может превышать 50 символов")]
        public string? Patronymic { get; set; }
    }

    public enum UserRole
    {
        User,
        Admin
    }

    public class User : Person
    {
        [Required(ErrorMessage = "Логин обязателен")]
        [Display(Name = "Логин")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Пароль (хэш)")]
        public string PasswordHash { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата рождения")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Номер телефона")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Роль")]
        public UserRole Role { get; set; } = UserRole.User;

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public string GetFullName()
        {
            return $"{LastName} {FirstName} {Patronymic}".Trim();
        }

        public string GetInitials()
        {
            var patronymicInitial = !string.IsNullOrEmpty(Patronymic) ? $"{Patronymic[0]}." : "";
            return $"{LastName} {FirstName[0]}.{patronymicInitial}".Trim();
        }

        public User() { }

        public User(string userName, string email, string passwordHash, UserRole role = UserRole.User)
        {
            UserName = userName;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            RegistrationDate = DateTime.UtcNow;
        }

        ~User() { }
    }
}