using System.Text.Json.Serialization;

namespace DulceRecetario.Shared.DTOs;

public class IngredientDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    
    public double Quantity { get; set; }

    [JsonIgnore]
    public string QuantityText
    {
        get => Quantity > 0 ? Quantity.ToString("0.##") : string.Empty;
        set => Quantity = ParseQuantity(value);
    }

    public string? Unit { get; set; }
    public int OrderIndex { get; set; }

    public string FullDescription =>
        string.IsNullOrWhiteSpace(Unit) ? $"{GetDisplayQuantity()} {Name}" : $"{GetDisplayQuantity()} {Unit} de {Name}";

    public string GetDisplayQuantity() => Quantity.ToString("0.##");

    public string GetScaledDescription(double factor)
    {
        double scaledQty = Quantity * factor;
        return string.IsNullOrWhiteSpace(Unit) ? $"{scaledQty:0.##} {Name}" : $"{scaledQty:0.##} {Unit} de {Name}";
    }

    private static double ParseQuantity(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        if (double.TryParse(text.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result))
            return result;

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
