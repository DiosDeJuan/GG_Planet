using System.Globalization;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Central USD money formatting/parsing helper.
    /// All gameplay money values are stored as cents.
    /// </summary>
    public static class MoneyFormatter
    {
        public static readonly CultureInfo UsdCulture = CreateUsdCulture();

        private static CultureInfo CreateUsdCulture()
        {
            CultureInfo culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            culture.NumberFormat.CurrencySymbol = "$";
            culture.NumberFormat.CurrencyDecimalDigits = 2;
            culture.NumberFormat.CurrencyDecimalSeparator = ".";
            culture.NumberFormat.CurrencyGroupSeparator = ",";
            culture.NumberFormat.CurrencyNegativePattern = 1;
            return culture;
        }

        public static string Format(long cents)
        {
            return (cents / 100m).ToString("C", UsdCulture);
        }

        public static string Format(decimal amount)
        {
            return amount.ToString("C", UsdCulture);
        }

        public static string Format(float amount)
        {
            return ((decimal)amount).ToString("C", UsdCulture);
        }

        public static string FormatShort(long cents)
        {
            decimal amount = cents / 100m;
            if (amount >= 1000000m)
                return "$" + (amount / 1000000m).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            if (amount >= 1000m)
                return "$" + (amount / 1000m).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            return Format(cents);
        }

        public static long ParseToCents(string money)
        {
            decimal value;
            if (!decimal.TryParse(money, NumberStyles.Currency, UsdCulture, out value))
                return 0L;
            return (long)(value * 100m);
        }
    }
}
