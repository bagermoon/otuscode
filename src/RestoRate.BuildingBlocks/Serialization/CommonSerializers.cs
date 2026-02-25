using System;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace RestoRate.BuildingBlocks.Serialization;

public class CommonSerializers
{
    private static int registered;

    public static void EnsureRegistered()
    {
        if (Interlocked.Exchange(ref registered, 1) == 1)
        {
            return;
        }

        MoneyBsonSerializer.EnsureRegistered();

        // MongoDB Driver requires an explicit Guid representation for serialization.
        // Registering serializers is the most compatible approach across driver versions.
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch
        {
            // ignore if already registered
        }

        try
        {
            BsonSerializer.RegisterSerializer(new NullableSerializer<Guid>(new GuidSerializer(GuidRepresentation.Standard)));
        }
        catch
        {
            // ignore if already registered
        }
    }
}
