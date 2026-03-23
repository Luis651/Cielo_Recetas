using DulceRecetario.DTOs;
using DulceRecetario.Models;
using Supabase;

namespace DulceRecetario.Services;

/// <summary>
/// Servicio CRUD para el manejo de recetas contra Supabase.
/// </summary>
public class RecipeService
{
    private readonly SupabaseService _supabaseService;

    public RecipeService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    // ── READ ─────────────────────────────────────────────────────────────

    public async Task<List<RecipeDto>> GetAllRecipesAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        var response = await client.From<Recipe>()
            .Order("created_at", Postgrest.Constants.Ordering.Descending)
            .Get();

        return response.Models.Select(MapToDto).ToList();
    }

    public async Task<List<RecipeDto>> GetFavoritesAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        var response = await client.From<Recipe>()
            .Filter("is_favorite", Postgrest.Constants.Operator.Equals, "true")
            .Order("name", Postgrest.Constants.Ordering.Ascending)
            .Get();

        return response.Models.Select(MapToDto).ToList();
    }

    public async Task<List<RecipeDto>> SearchRecipesAsync(string query)
    {
        var client = await _supabaseService.GetClientAsync();
        var response = await client.From<Recipe>()
            .Filter("name", Postgrest.Constants.Operator.ILike, $"%{query}%")
            .Get();

        return response.Models.Select(MapToDto).ToList();
    }

    public async Task<RecipeDto?> GetRecipeByIdAsync(Guid id)
    {
        var client = await _supabaseService.GetClientAsync();
        var response = await client.From<Recipe>()
            .Filter("id", Postgrest.Constants.Operator.Equals, id.ToString())
            .Single();

        return response is null ? null : MapToDto(response);
    }

    // ── CREATE ────────────────────────────────────────────────────────────

    public async Task<RecipeDto?> CreateRecipeAsync(RecipeDto dto)
    {
        var client = await _supabaseService.GetClientAsync();
        var model = MapToModel(dto);
        model.Id = Guid.NewGuid();
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;

        var response = await client.From<Recipe>().Insert(model);
        return response.Models.FirstOrDefault() is { } created ? MapToDto(created) : null;
    }

    // ── UPDATE ────────────────────────────────────────────────────────────

    public async Task<RecipeDto?> UpdateRecipeAsync(RecipeDto dto)
    {
        var client = await _supabaseService.GetClientAsync();
        var model = MapToModel(dto);
        model.UpdatedAt = DateTime.UtcNow;

        var response = await client.From<Recipe>().Upsert(model);
        return response.Models.FirstOrDefault() is { } updated ? MapToDto(updated) : null;
    }

    public async Task ToggleFavoriteAsync(Guid id, bool isFavorite)
    {
        var client = await _supabaseService.GetClientAsync();
        await client.From<Recipe>()
            .Filter("id", Postgrest.Constants.Operator.Equals, id.ToString())
            .Set(r => r.IsFavorite, isFavorite)
            .Update();
    }

    // ── DELETE ────────────────────────────────────────────────────────────

    public async Task DeleteRecipeAsync(Guid id)
    {
        var client = await _supabaseService.GetClientAsync();
        await client.From<Recipe>()
            .Filter("id", Postgrest.Constants.Operator.Equals, id.ToString())
            .Delete();
    }

    // ── MAPPING ───────────────────────────────────────────────────────────

    private static RecipeDto MapToDto(Recipe model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        Category = model.Category,
        Difficulty = model.Difficulty,
        PrepTimeMinutes = model.PrepTimeMinutes,
        Servings = model.Servings,
        ImageUrl = model.ImageUrl,
        IsFavorite = model.IsFavorite,
        CreatedAt = model.CreatedAt
    };

    private static Recipe MapToModel(RecipeDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Category = dto.Category,
        Difficulty = dto.Difficulty,
        PrepTimeMinutes = dto.PrepTimeMinutes,
        Servings = dto.Servings,
        ImageUrl = dto.ImageUrl,
        IsFavorite = dto.IsFavorite
    };
}
