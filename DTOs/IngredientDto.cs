namespace DulceRecetario.DTOs;

public class IngredientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int OrderIndex { get; set; }

    // Display helper
    public string FullDescription =>
        string.IsNullOrWhiteSpace(Unit) ? $"{Quantity} {Name}" : $"{Quantity} {Unit} de {Name}";
}

public class RecipeStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}
