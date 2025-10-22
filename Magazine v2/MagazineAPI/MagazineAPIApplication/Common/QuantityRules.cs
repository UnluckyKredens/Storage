using MagazineAPIApplication.Common.Exceptions;

namespace MagazineAPIApplication.Common;

public static class QuantityRules
{
    public const decimal MaxQuantity = 1_000_000m;
    private const int MaxDecimalPlaces = 3;

    private static readonly HashSet<string> FractionalSymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        "kg", "g", "t", "m", "mb", "cm", "mm", "l", "ml", "m2", "m3"
    };

    public static bool AllowsFraction(string? unitSymbol) =>
        !string.IsNullOrWhiteSpace(unitSymbol) && FractionalSymbols.Contains(unitSymbol.Trim());

    public static void Ensure(decimal quantity, string? unitSymbol, bool allowZero = false)
    {
        if (quantity < 0 || (!allowZero && quantity == 0))
            throw new CommandValidationException("Ilość musi być większa od zera.");
        if (quantity > MaxQuantity)
            throw new CommandValidationException($"Ilość nie może przekraczać {MaxQuantity:N0}.");
        if (decimal.Round(quantity, MaxDecimalPlaces) != quantity)
            throw new CommandValidationException($"Ilość może mieć najwyżej {MaxDecimalPlaces} miejsca po przecinku.");
        if (quantity != decimal.Truncate(quantity) && !AllowsFraction(unitSymbol))
            throw new CommandValidationException("Ilość musi być liczbą całkowitą dla tej jednostki miary.");
    }
}
