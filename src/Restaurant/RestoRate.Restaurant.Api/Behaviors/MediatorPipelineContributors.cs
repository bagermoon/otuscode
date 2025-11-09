using System.Runtime.CompilerServices;

using Ardalis.SharedKernel;

using RestoRate.SharedKernel.Mediator;

namespace RestoRate.Restaurant.Api.Behaviors;

public static class MediatorPipelineContributors
{
#pragma warning disable CA2255 // 'ModuleInitializer' in library: intentional for startup wiring
    [ModuleInitializer]
    public static void Register()
    {
        MediatorPipelineRegistry.Register(typeof(LoggingBehavior<,>));
    }
#pragma warning restore CA2255
}
