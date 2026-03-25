using System.Text.Json;
using System.Diagnostics;
using System.IO;
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
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RecipeService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    // ── READ ─────────────────────────────────────────────────────────────

    public async Task<List<RecipeDto>> GetAllRecipesAsync()
    {
        try 
        {
            var client = await _supabaseService.GetClientAsync();
            var response = await client.From<Recipe>()
                .Order("created_at", Postgrest.Constants.Ordering.Descending)
                .Get();

            var dtos = new List<RecipeDto>();
            foreach (var model in response.Models)
            {
                try 
                {
                    dtos.Add(MapToDto(model));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error mapeando receta {model.Id}: {ex.Message}");
                }
            }
            return dtos;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error en GetAllRecipesAsync: {ex.Message}");
            return new List<RecipeDto>();
        }
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

    // ── STORAGE ──────────────────────────────────────────────────────────

    public async Task<string?> UploadImageAsync(Stream imageStream, string fileName)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            var storage = client.Storage;
            var bucket = storage.From("recetas");

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                await imageStream.CopyToAsync(memoryStream);
                bytes = memoryStream.ToArray();
            }

            await bucket.Upload(bytes, fileName, new Supabase.Storage.FileOptions { Upsert = true });
            return bucket.GetPublicUrl(fileName);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error subiendo imagen: {ex.Message}");
            return null;
        }
    }

    // ── MAPPING ───────────────────────────────────────────────────────────

    /// <summary>
    /// Intenta parsear un campo JSON de Supabase que puede venir como un JArray nativo o como un string ("[{...}]").
    /// </summary>
    private static List<T> ParseMixedJsonbArray<T>(object? rawValue)
    {
        if (rawValue == null) return new List<T>();

        try
        {
            var jsonString = rawValue.ToString();
            if (string.IsNullOrWhiteSpace(jsonString)) return new List<T>();

            // Si vino como string escapado tipo "\[{...}\]", ToString() nos dará la cadena limpia
            // Si vino como JArray nativo, ToString() suele dar el JSON real "[{...}]"
            return JsonSerializer.Deserialize<List<T>>(jsonString, _jsonOptions) ?? new List<T>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error parseando JSON: {ex.Message}");
            return new List<T>();
        }
    }

    private static RecipeDto MapToDto(Recipe model)
    {
        return new RecipeDto
        {
            Id = model.Id,
            Name = model.Name ?? "Sin nombre",
            Description = model.Description ?? "",
            Category = model.Category ?? "Otros",
            Difficulty = model.Difficulty ?? "Fácil",
            PrepTimeMinutes = model.PrepTimeMinutes,
            Servings = model.Servings ?? 1,
            BaseMoldSize = model.BaseMoldSize ?? 20.0,
            ImageUrl = model.ImageUrl,
            IsFavorite = model.IsFavorite,
            CreatedAt = model.CreatedAt,
            // Parseamos robustamente desde object? (puede ser string o JArray)
            Ingredients = ParseMixedJsonbArray<IngredientDto>(model.IngredientsRaw),
            Steps = ParseMixedJsonbArray<RecipeStepDto>(model.StepsRaw)
        };
    }

    private static Recipe MapToModel(RecipeDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Description = dto.Description,
        Category = dto.Category,
        Difficulty = dto.Difficulty,
        PrepTimeMinutes = dto.PrepTimeMinutes,
        Servings = dto.Servings,
        BaseMoldSize = dto.BaseMoldSize,
        ImageUrl = dto.ImageUrl,
        IsFavorite = dto.IsFavorite,
        // Al guardar enviamos diccionarios o serializamos a C# Types. 
        // Postgrest prefiere arrays u objetos definidos para columnas JSONB, 
        // pero podemos mandar una cadena serializada y dejar que Postgrest la guarde como string.
        IngredientsRaw = JsonSerializer.Serialize(dto.Ingredients, _jsonOptions),
        StepsRaw = JsonSerializer.Serialize(dto.Steps, _jsonOptions)
    };
}
