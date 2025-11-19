using FluentValidation;
using RestoRate.Restaurant.Application.UseCases.Create;

namespace RestoRate.Restaurant.Application.Validators;

public class CreateRestaurantValidator : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Имя ресторана обязательно")
            .MinimumLength(3).WithMessage("Имя должно содержать минимум 3 символа")
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");

        RuleFor(x => x.Dto.Description)
            .NotEmpty().WithMessage("Описание обязательно")
            .MinimumLength(10).WithMessage("Описание должно содержать минимум 10 символов");

        RuleFor(x => x.Dto.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона обязателен")
            .Matches(@"^\d{3,15}$").WithMessage("Неверный формат телефона");

        RuleFor(x => x.Dto.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Неверный формат email");

        RuleFor(x => x.Dto.FullAddress)
            .NotEmpty().WithMessage("Адрес ресторана обязателен");

        RuleFor(x => x.Dto.House)
            .NotEmpty().WithMessage("Адресный дом ресторана обязателен");

        RuleFor(x => x.Dto.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Широта должна быть между -90 и 90");

        RuleFor(x => x.Dto.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Долгота должна быть между -180 и 180");

        RuleFor(x => x.Dto.DayOfWeek)
            .NotEmpty().WithMessage("Дни недели ресторана обязателен");

        RuleFor(x => x.Dto.DayOfWeek)
            .NotEmpty().WithMessage("Время открытия ресторана обязателен");

        RuleFor(x => x.Dto.DayOfWeek)
            .NotEmpty().WithMessage("Время закрытия ресторана обязателен");

        RuleFor(x => x.Dto.CuisineType)
            .NotEmpty().WithMessage("Тип кухни обязателен");

        RuleFor(x => x.Dto.AverageCheckAmount)
            .GreaterThan(0).WithMessage("Средний чек должен быть больше 0");

        RuleFor(x => x.Dto.AverageCheckCurrency)
            .NotEmpty().WithMessage("Валюта обязательна")
            .Length(3).WithMessage("Код валюты должен быть 3 символа");

        RuleFor(x => x.Dto.Tag)
            .NotEmpty().WithMessage("Тип ресторана обязателен");
    }
}
