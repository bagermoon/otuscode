using System.Reflection;

using Ardalis.Result;
using Ardalis.Result.FluentValidation;

using FluentValidation;
using FluentValidation.Results;

using Mediator;

namespace RestoRate.Abstractions.Mediation.Behaviors;

public class ValidationBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TMessage request, MessageHandlerDelegate<TMessage,
        TResponse> next,
        CancellationToken cancellationToken)
    {
        // если нет валидаторов, ничего не делаем
        if (!_validators.Any())
            return await next(request, cancellationToken);

        // валидация запроса
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(new ValidationContext<TMessage>(request), cancellationToken)));

        var failures = validationResults
                .Where(r => r.Errors.Count > 0)
                .SelectMany(r => r.Errors)
                .ToList();

        // если нет ошибок, ничего не делаем
        if (failures.Count == 0)
            return await next(request, cancellationToken);

        var errors = new ValidationResult(failures).AsErrors();

        // Выбрасывание исключения - дорогой способ обработки ошибок валидации,
        // поэтому мы пытаемся вернуть Result или Result<T>, если это возможно.

        // сначала проверяем, является ли TResponse типом Result
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Invalid(errors);
        }

        // затем проверяем, является ли TResponse типом Result<T>
        var responseType = typeof(TResponse);
        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArg = responseType.GetGenericArguments()[0];
            var genericResultType = typeof(Result<>).MakeGenericType(genericArg);

            var invalidMethod = genericResultType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m =>
                    m.Name == "Invalid"
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(IEnumerable<ValidationError>)));

            return (TResponse)invalidMethod!.Invoke(null, [errors])!;
        }

        // Если TResponse не является ни Result, ни Result<T>, выбрасываем исключение
        throw new ValidationException(failures);
    }
}
