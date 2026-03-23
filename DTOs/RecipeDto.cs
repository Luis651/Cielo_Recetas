namespace DulceRecetario.DTOs;

public class RecipeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Difficulty { get; set; } = "Fácil";
    public int PrepTimeMinutes { get; set; }
    public int Servings { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<RecipeStepDto> Steps { get; set; } = new();
}
