using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;

using NodaMoney;

namespace RestoRate.BuildingBlocks.Serialization;

public sealed class MoneyBsonSerializer : SerializerBase<Money>
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

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Money value)
    {
        context.Writer.WriteStartDocument();

        context.Writer.WriteName("Amount");
        context.Writer.WriteDecimal128(new Decimal128(value.Amount));

        context.Writer.WriteName("Currency");
        context.Writer.WriteString(value.Currency.Code);

        context.Writer.WriteEndDocument();
    }

    public override Money Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;
        reader.ReadStartDocument();

        decimal amount = 0m;
        string currencyCode = string.Empty;

        while (reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = reader.ReadName();
            switch (name)
            {
                case "Amount":
                    amount = Decimal128.ToDecimal(reader.ReadDecimal128());
                    break;
                case "Currency":
                    currencyCode = reader.ReadString();
                    break;
                default:
                    reader.SkipValue();
                    break;
            }
        }

        reader.ReadEndDocument();

        return new Money(amount, Currency.FromCode(currencyCode));
    }
}
