using System.Text.Json.Serialization;

namespace DulceRecetario.DTOs;

public class IngredientDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    
    // Numeric value for calculations
    public double Quantity { get; set; }

    // Text value for UI (supports "1/2", "0.5", etc.)
    [JsonIgnore]
    public string QuantityText
    {
        get => Quantity > 0 ? Quantity.ToString("0.##") : string.Empty;
        set => Quantity = ParseQuantity(value);
    }

    public string? Unit { get; set; }
    public int OrderIndex { get; set; }

    // Display helper for original values
    public string FullDescription =>
        string.IsNullOrWhiteSpace(Unit) ? $"{GetDisplayQuantity()} {Name}" : $"{GetDisplayQuantity()} {Unit} de {Name}";

    public string GetDisplayQuantity() => Quantity.ToString("0.##");

    // Helper for scaled values
    public string GetScaledDescription(double factor)
    {
        double scaledQty = Quantity * factor;
        return string.IsNullOrWhiteSpace(Unit) ? $"{scaledQty:0.##} {Name}" : $"{scaledQty:0.##} {Unit} de {Name}";
    }

    private static double ParseQuantity(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        // Try simple number
        if (double.TryParse(text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
            return result;

        // Try fraction like "1/2"
        if (text.Contains('/'))
        {
            var parts = text.Split('/');
            if (parts.Length == 2 && 
                double.TryParse(parts[0], out double num) && 
                double.TryParse(parts[1], out double den) && den != 0)
            {
                return num / den;
            }
        }

        return 0;
    }
}
