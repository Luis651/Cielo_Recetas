using Postgrest.Attributes;
using Postgrest.Models;

namespace DulceRecetario.Shared.Models;

[Table("ingredients")]
public class Ingredient : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("quantity")]
    public string Quantity { get; set; } = string.Empty;

    [Column("unit")]
    public string? Unit { get; set; }

    [Column("order_index")]
    public int OrderIndex { get; set; }
}
