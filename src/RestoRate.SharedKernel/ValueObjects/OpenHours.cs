using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel.ValueObjects
{
    public class OpenHours : ValueObject
    {
        public DayOfWeek DayOfWeek { get; }
        public TimeOnly OpenTime { get; }
        public TimeOnly CloseTime { get; }
        public bool IsClosed { get; }

        public OpenHours(DayOfWeek dayOfWeek, TimeOnly openTime, TimeOnly closeTime, bool isClosed = false)
        {
            DayOfWeek = Guard.Against.EnumOutOfRange(dayOfWeek, nameof(dayOfWeek));
            IsClosed = isClosed;

            if (!isClosed)
            {
                OpenTime = Guard.Against.Default(openTime, nameof(openTime));
                CloseTime = Guard.Against.Default(closeTime, nameof(closeTime));

                if (closeTime <= openTime)
                    throw new ArgumentException("Время закрытия должно быть позже времени открытия", nameof(closeTime));
            }
            else
            {
                OpenTime = TimeOnly.MinValue;
                CloseTime = TimeOnly.MinValue;
            }
        }

        /// <summary> Закрыт или выходной </summary>
        public static OpenHours Closed(DayOfWeek dayOfWeek) => new(dayOfWeek, TimeOnly.MinValue, TimeOnly.MinValue, true);
        /// <summary> Круглосуточный  </summary>
        public static OpenHours Open24Hours(DayOfWeek dayOfWeek) => new(dayOfWeek, TimeOnly.MinValue, new TimeOnly(23, 59), false);
        /// <summary> Время работы  </summary>
        public bool IsOpenAt(TimeOnly time)
        {
            if (IsClosed) return false;
            return time >= OpenTime && time <= CloseTime;
        }

        public string GetScheduleString() => IsClosed
            ? $"{DayOfWeek}: Выходной"
            : $"{DayOfWeek}: {OpenTime:HH:mm} - {CloseTime:HH:mm}";

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return DayOfWeek;
            yield return OpenTime;
            yield return CloseTime;
            yield return IsClosed;
        }

        public override string ToString() => GetScheduleString();
    }
}
