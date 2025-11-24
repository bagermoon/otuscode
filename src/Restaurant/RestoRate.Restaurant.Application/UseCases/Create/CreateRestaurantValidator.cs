using FluentValidation;

namespace RestoRate.Restaurant.Application.UseCases.Create;

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

        RuleFor(x => x.Dto.Address)
            .NotEmpty().WithMessage("Адрес ресторана обязателен");

        RuleFor(x => x.Dto.Location.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Широта должна быть между -90 и 90");

        RuleFor(x => x.Dto.Location.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Долгота должна быть между -180 и 180");

        RuleFor(x => x.Dto.OpenHours.DayOfWeek)
            .NotEmpty().WithMessage("Указать дни недели работы ресторана, обязателено");

        RuleFor(x => x.Dto.OpenHours.DayOfWeek)
            .NotEmpty().WithMessage("Указать время открытия ресторана, обязателено");

        RuleFor(x => x.Dto.OpenHours.DayOfWeek)
            .NotEmpty().WithMessage("Указать время закрытия ресторана, обязателено");

        RuleFor(x => x.Dto.AverageCheck.Amount)
            .GreaterThan(0).WithMessage("Средний чек должен быть больше 0");

        RuleFor(x => x.Dto.AverageCheck.Currency)
            .NotEmpty().WithMessage("Валюта обязательна")
            .Length(3).WithMessage("Код валюты должен быть 3 символа");

        RuleFor(x => x.Dto.CuisineTypes)
            .NotEmpty().WithMessage("Тип кухни обязателен");

        RuleFor(x => x.Dto.Tags)
            .NotEmpty().WithMessage("Тег ресторана обязателен");

        RuleFor(x => x.Dto.Images)
           .Must(images => images == null || images.Count <= 10)
           .WithMessage("Максимум 10 изображений");


        RuleForEach(x => x.Dto.Images)
            .ChildRules(image =>
            {
                image.RuleFor(x => x.Url)
                    .NotEmpty().WithMessage("URL изображения обязателен")
                    .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .WithMessage("Неверный формат URL изображения");
            })
            .When(x => x.Dto.Images != null);
    }
}
