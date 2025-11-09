using System.Runtime.CompilerServices;
using RestoRate.SharedKernel.Mediator;

namespace RestoRate.Restaurant.Application.Behaviors;

public static class MediatorPipelineContributors
{
#pragma warning disable CA2255 // 'ModuleInitializer' in library: intentional for startup wiring
    [ModuleInitializer]
    public static void Register()
    {
        MediatorPipelineRegistry.Register(typeof(ValidationBehavior<,>));
    }
#pragma warning restore CA2255
}
