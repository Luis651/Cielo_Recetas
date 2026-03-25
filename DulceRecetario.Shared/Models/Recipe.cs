using Postgrest.Attributes;
using Postgrest.Models;

namespace DulceRecetario.Shared.Models;

[Table("recipes")]
public class Recipe : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public string? UserId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("category")]
    public string? Category { get; set; }

    [Column("difficulty")]
    public string Difficulty { get; set; } = "Fácil";

    [Column("prep_time_minutes")]
    public int PrepTimeMinutes { get; set; }

    [Column("servings")]
    public int? Servings { get; set; }

    [Column("base_mold_size")]
    public double? BaseMoldSize { get; set; }

    [Column("ingredients_json")]
    public object? IngredientsRaw { get; set; }

    [Column("steps_json")]
    public object? StepsRaw { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("is_favorite")]
    public bool IsFavorite { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
