using System;
using System.Collections.Generic;

namespace RestoRate.SharedKernel.Mediator;

public static class MediatorPipelineRegistry
{
    private static readonly List<Type> Behaviors = new();

    public static void Register(Type openGenericBehaviorType)
    {
        Behaviors.Add(openGenericBehaviorType);
    }

    public static Type[] GetAll() => Behaviors.ToArray();
}
