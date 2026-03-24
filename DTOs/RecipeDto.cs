using System.Text.Json.Serialization;

namespace DulceRecetario.DTOs;

public class RecipeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Difficulty { get; set; } = "Fácil";
    public int PrepTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;

    // Nuevo: Tamaño de molde base (cm) para escalado
    public double BaseMoldSize { get; set; } = 20.0;

    public string? ImageUrl { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<IngredientDto> Ingredients { get; set; } = new();
    
    // Nuevo: Pasos de la receta
    public List<RecipeStepDto> Steps { get; set; } = new();
}
