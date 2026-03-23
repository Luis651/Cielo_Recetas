using Postgrest.Attributes;
using Postgrest.Models;

namespace DulceRecetario.Models;

[Table("recipe_steps")]
public class RecipeStep : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    [Column("step_number")]
    public int StepNumber { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;
}
