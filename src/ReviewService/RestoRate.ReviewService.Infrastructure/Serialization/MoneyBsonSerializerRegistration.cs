using System.Threading;

using MongoDB.Bson.Serialization;

using NodaMoney;

namespace RestoRate.ReviewService.Infrastructure.Serialization;

internal static class MoneyBsonSerializerRegistration
{
    private static int registered;

    public static void EnsureRegistered()
    {
        if (Interlocked.Exchange(ref registered, 1) == 1)
        {
            return;
        }

        BsonSerializer.RegisterSerializer<Money>(new MoneyBsonSerializer());
    }
}
