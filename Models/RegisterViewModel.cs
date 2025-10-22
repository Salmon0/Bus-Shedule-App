using System.ComponentModel.DataAnnotations;
public class RegisterViewModel
{
    [Required(ErrorMessage = "Email обязателен для заполнения")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Пароль обязателен для заполнения")]
    [StringLength(100, ErrorMessage = "Пароль должен содержать от {2} до {1} символов", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Пароль должен содержать цифры, заглавные и строчные буквы, и спецсимволы")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [StringLength(50, ErrorMessage = "Имя не может превышать {1} символов")]
    [Display(Name = "Имя")]
    [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\-]+$", ErrorMessage = "Имя может содержать только буквы и дефис")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Фамилия обязательна для заполнения")]
    [StringLength(50, ErrorMessage = "Фамилия не может превышать {1} символов")]
    [Display(Name = "Фамилия")]
    [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\-]+$", ErrorMessage = "Фамилия может содержать только буквы и дефис")]
    public string LastName { get; set; }

    [StringLength(50, ErrorMessage = "Отчество не может превышать {1} символов")]
    [Display(Name = "Отчество (при наличии)")]
    [RegularExpression(@"^[а-яА-ЯёЁa-zA-Z\-]*$", ErrorMessage = "Отчество может содержать только буквы и дефис")]
    public string? Patronymic { get; set; }

    [Display(Name = "Дата рождения")]
    [DataType(DataType.Date)]
    [AgeValidation(14, ErrorMessage = "Вы должны быть старше 14 лет")]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "Номер телефона")]
    [Phone(ErrorMessage = "Некорректный формат номера телефона")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Необходимо принять условия соглашения")]
    [Display(Name = "Я принимаю условия пользовательского соглашения")]
    [MustBeTrue(ErrorMessage = "Вы должны принять условия соглашения")]
    public bool AcceptTerms { get; set; }
}

// Кастомный атрибут валидации для возраста
public class AgeValidationAttribute : ValidationAttribute
{
    private readonly int _minimumAge;

    public AgeValidationAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime birthDate)
        {
            var age = DateTime.Today.Year - birthDate.Year;
            if (birthDate > DateTime.Today.AddYears(-age)) age--;

            if (age < _minimumAge)
            {
                return new ValidationResult(ErrorMessage);
            }
        }
        return ValidationResult.Success;
    }
}

// Кастомный атрибут для проверки принятия условий
public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        return value is bool b && b;
    }
}