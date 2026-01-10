using System;

using Microsoft.Extensions.DependencyInjection;

using NodaMoney;
using NodaMoney.DependencyInjection;

namespace RestoRate.ServiceDefaults;

public static class MoneyExtensions
{
    public static IServiceCollection AddMoneyDefaults(this IServiceCollection services)
    {
        // TODO : Make currency configurable (move to AppHost, configure tests, etc.)
        services.AddMoneyContext(options =>
        {
            options.DefaultCurrency = CurrencyInfo.Create("RUB");
            options.Precision = 18;
        });
        return services;
    }
}
