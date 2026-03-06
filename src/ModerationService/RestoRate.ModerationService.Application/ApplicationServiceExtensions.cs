using Microsoft.Extensions.DependencyInjection;

using RestoRate.ModerationService.Application.Engine;
using RestoRate.ModerationService.Application.Interfaces;
using RestoRate.ModerationService.Application.Rules;

namespace RestoRate.ModerationService.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddModerationApplication(this IServiceCollection services)
    {
        services.AddSingleton<IModerationRule, BadWordsRule>();
        services.AddSingleton<IModerationRule, SpamRule>();

        services.AddSingleton<ITextModerator, TextModerator>();

        return services;
    }
}
