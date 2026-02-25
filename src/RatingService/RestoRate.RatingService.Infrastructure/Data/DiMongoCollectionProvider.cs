using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;
/// <summary>
/// Простая <see cref="IMongoCollectionProvider"/> реализация, которая получает коллекции из DI контейнера.
/// Используется <see cref="MongoContext"/> для получения коллекций для агрегатов.
/// Может быть использована для расширения функционала получения коллекций и тестирования.
/// </summary>
internal sealed class DiMongoCollectionProvider(IServiceProvider serviceProvider) : IMongoCollectionProvider
{
    public IMongoCollection<T> GetRequiredCollection<T>() where T : class
        => serviceProvider.GetRequiredService<IMongoCollection<T>>();
}
