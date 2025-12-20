using System;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel.ValueObjects
{
    public class Address : ValueObject
    {
        /// <summary>Полная строка адреса до улицы</summary>
        public string FullAddress { get; set; } = string.Empty;
        /// <summary> Дом </summary>
        public string House { get; set; }

        public Address(string fullAddress, string house)
        {
            FullAddress = Guard.Against.NullOrWhiteSpace(fullAddress, nameof(fullAddress));
            House = Guard.Against.NullOrWhiteSpace(house, nameof(house));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FullAddress;
            yield return House;
        }

        public override string ToString() => FullAddress;
    }
}
