using CrystalSharp.Domain;
using CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate.Events;

namespace CrystalSharp.Tests.Common.MsSql.Aggregates.CurrencyAggregate
{
    public class Currency : AggregateRoot<int>
    {
        public string Name { get; private set; }
        public CurrencyDetails CurrencyDetails { get; private set; }

        private static void ValidateCurrency(Currency currency)
        {
            if (string.IsNullOrEmpty(currency.Name))
            {
                currency.ThrowDomainException("Currency name is required.");
            }

            if (currency.CurrencyDetails == null)
            {
                currency.ThrowDomainException("Currency details are required.");
            }

            if (string.IsNullOrEmpty(currency.CurrencyDetails.Code))
            {
                currency.ThrowDomainException("Currency code is required.");
            }

            if (currency.CurrencyDetails.Code.Length > 3)
            {
                currency.ThrowDomainException("Only three characters allowed for code.");
            }

            if (currency.CurrencyDetails.NumericCode <= 0)
            {
                currency.ThrowDomainException("Currency numeric code should be greater than 0.");
            }

            if (currency.CurrencyDetails.NumericCode.ToString().Length > 3)
            {
                currency.ThrowDomainException("Only three digits allowed for numeric code.");
            }
        }

        public static string GetSampleCurrencyName()
        {
            string name = "Sample";

            return name;
        }

        public static string GetTestCurrencyName()
        {
            string name = "TEST";

            return name;
        }

        public static Currency Create(string name, CurrencyDetails currencyDetails)
        {
            Currency currency = new() { Name = name, CurrencyDetails = currencyDetails };

            ValidateCurrency(currency);

            currency.Raise(new CurrencyCreatedDomainEvent(currency.GlobalUId, currency.Name, currencyDetails));

            return currency;
        }

        public void ChangeName(string name)
        {
            Name = name;

            Raise(new CurrencyNameChangedDomainEvent(GlobalUId, Name));
        }

        public void ChangeDetails(CurrencyDetails currencyDetails)
        {
            CurrencyDetails = currencyDetails;

            ValidateCurrency(this);

            Raise(new CurrencyDetailsChangedDomainEvent(GlobalUId, CurrencyDetails));
        }

        public override void Delete()
        {
            base.Delete();
            Raise(new CurrencyDeletedDomainEvent(GlobalUId, Name, CurrencyDetails));
        }

        private void Apply(CurrencyCreatedDomainEvent @event)
        {
            Name = @event.Name;
            CurrencyDetails = @event.CurrencyDetails;
        }

        private void Apply(CurrencyNameChangedDomainEvent @event)
        {
            Name = @event.Name;
        }

        private void Apply(CurrencyDetailsChangedDomainEvent @event)
        {
            CurrencyDetails = @event.CurrencyDetails;
        }

        private void Apply(CurrencyDeletedDomainEvent @event)
        {
            Name = @event.Name;
            CurrencyDetails = @event.CurrencyDetails;
        }
    }
}
