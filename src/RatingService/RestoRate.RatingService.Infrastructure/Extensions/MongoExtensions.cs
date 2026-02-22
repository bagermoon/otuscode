using System.Linq.Expressions;
using System.Reflection;

using Ardalis.SharedKernel;

using MassTransit.MongoDbIntegration.Saga;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using RestoRate.Abstractions.Persistence;
using RestoRate.RatingService.Infrastructure.Configuration;
using RestoRate.RatingService.Infrastructure.Data;
using RestoRate.SharedKernel;

namespace RestoRate.RatingService.Infrastructure.Extensions;

public static class MongoExtensions
{
    public static IServiceCollection AddMongoContext<TContext>(this IServiceCollection services)
        where TContext : IMongoContext
    {
        services.TryAddScoped<MongoUnitOfWork>();
        services.TryAddScoped<ISessionHolder, EmptySessionHolder>();
        services.TryAddScoped<ISessionContext>(sp => sp.GetRequiredService<ISessionHolder>());
        services.TryAddTransient<IMongoCollectionProvider, DiMongoCollectionProvider>();
        services.TryAddScoped(typeof(IMongoContext), typeof(TContext));
        services.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MongoUnitOfWork>());

        return services;
    }
    public static IServiceCollection AddMongoDbCollection<T>(
        this IServiceCollection services,
        Expression<Func<T, Guid>>? idPropertyExpression = null,
        Action<IMongoCollection<T>>? initiateCollection = null)
        where T : class
    {
        TryRegisterAggregateWriter<T>(services);

        IMongoCollection<T> MongoDbCollectionFactory(IServiceProvider provider)
        {
            var database = provider.GetRequiredService<IMongoDatabase>();
            var collectionNameFormatter = DotCaseCollectionNameFormatter.Instance;
            var collection = database.GetCollection<T>(collectionNameFormatter.Collection<T>());

            if (BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                initiateCollection?.Invoke(collection);
                return collection;
            }

            BsonClassMap.RegisterClassMap<T>(cfg =>
            {
                cfg.AutoMap();
                if (idPropertyExpression != null)
                {
                    cfg.MapIdProperty(idPropertyExpression);
                }
            });

            initiateCollection?.Invoke(collection);

            return collection;
        }

        services.TryAddSingleton(MongoDbCollectionFactory);

        return services;
    }

    public static IServiceCollection AddConfiguredMongoDbCollections(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var openConfig = typeof(IMongoCollectionConfiguration<>);

        var configs = assemblies
            .Where(a => a is not null)
            .Distinct()
            .SelectMany(GetLoadableTypes)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openConfig)
                .Select(i => new
                {
                    Implementation = t,
                    EntityType = i.GetGenericArguments()[0]
                }));

        foreach (var config in configs)
        {
            AddConfiguredMongoDbCollection(services, config.Implementation, config.EntityType);
        }

        return services;
    }
    public static IServiceCollection AddConfiguredMongoDbCollection<T>(
        this IServiceCollection services,
        Action<IMongoCollection<T>>? initiateCollection = null)
        where T : class
    {
        TryRegisterAggregateWriter<T>(services);

        services.TryAddSingleton((IServiceProvider provider) =>
        {
            var database = provider.GetRequiredService<IMongoDatabase>();
            var collectionNameFormatter = DotCaseCollectionNameFormatter.Instance;
            var collection = database.GetCollection<T>(collectionNameFormatter.Collection<T>());

            var configuration = provider.GetService<IMongoCollectionConfiguration<T>>();

            if (configuration is not null)
            {
                configuration.Configure(collection);
            }
            else if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cfg => cfg.AutoMap());
            }

            initiateCollection?.Invoke(collection);
            return collection;
        });

        return services;
    }

    private static void TryRegisterAggregateWriter<T>(IServiceCollection services)
        where T : class
    {
        if (!typeof(EntityBase<Guid>).IsAssignableFrom(typeof(T)))
        {
            return;
        }

        var writerType = typeof(MongoAggregateWriter<>).MakeGenericType(typeof(T));
        services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMongoAggregateWriter), writerType));
    }

    private static IServiceCollection AddConfiguredMongoDbCollection(
        this IServiceCollection services,
        Type mongoConfigurationType, Type entityType)
    {
        var configServiceType = typeof(IMongoCollectionConfiguration<>).MakeGenericType(entityType);
        services.TryAddTransient(configServiceType, mongoConfigurationType);

        var methodInfo = typeof(MongoExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.IsGenericMethodDefinition && m.Name == nameof(AddConfiguredMongoDbCollection))
            .Select(m => new { Method = m, Parameters = m.GetParameters() })
            .FirstOrDefault(x =>
            {
                if (x.Parameters.Length != 2) return false;
                if (x.Parameters[0].ParameterType != typeof(IServiceCollection)) return false;

                var second = x.Parameters[1].ParameterType;
                if (!second.IsGenericType) return false;
                return second.GetGenericTypeDefinition() == typeof(Action<>);
            })?.Method;

        var genericMethod = methodInfo?.MakeGenericMethod(entityType);
        genericMethod?.Invoke(null, new object?[] { services, null });

        return services;
    }
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}