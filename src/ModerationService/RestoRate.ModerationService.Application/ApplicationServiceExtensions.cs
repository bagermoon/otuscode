using Microsoft.Extensions.DependencyInjection;

using RestoRate.ModerationService.Application.Configurations;
using RestoRate.ModerationService.Domain.Abstractions;
using RestoRate.ModerationService.Domain.Rules;
using RestoRate.ModerationService.Domain.Services;

namespace RestoRate.ModerationService.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddModerationApplication(this IServiceCollection services)
    {
        services.AddMediatorConfigs();

        services.AddSingleton<IModerationRule, BadWordsRule>();
        services.AddSingleton<IModerationRule, SpamRule>();
        services.AddSingleton<ITextModerator, TextModerator>();

        return services;
    }
}
