using System.Collections.ObjectModel;
using System.Windows.Input;
using DulceRecetario.DTOs;
using DulceRecetario.Services;

namespace DulceRecetario.ViewModels;

[QueryProperty(nameof(Refresh), "refresh")]
public class RecipeListViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;

    public ObservableCollection<RecipeDto> Recipes { get; } = new();

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            SetProperty(ref _searchQuery, value);
            _ = SearchAsync();
        }
    }

    private bool _refresh;
    public bool Refresh
    {
        get => _refresh;
        set { _refresh = value; if (value) _ = LoadRecipesAsync(); }
    }

    public ICommand LoadCommand { get; }
    public ICommand AddRecipeCommand { get; }
    public ICommand SelectRecipeCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }

    public RecipeListViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Title = "Mis Recetas";

        LoadCommand = new Command(async () => await LoadRecipesAsync());
        AddRecipeCommand = new Command(async () => await NavigateToAsync("RecipeFormPage"));
        SelectRecipeCommand = new Command<RecipeDto>(async r => await NavigateToAsync("RecipeDetailPage",
            new Dictionary<string, object> { ["RecipeId"] = r.Id }));
        ToggleFavoriteCommand = new Command<RecipeDto>(async r => await ToggleFavoriteAsync(r));
    }

    public async Task LoadRecipesAsync()
    {
        try
        {
            IsRefreshing = true;
            var list = await _recipeService.GetAllRecipesAsync();
            Recipes.Clear();
            foreach (var r in list) Recipes.Add(r);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error al cargar recetas: " + ex.Message;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadRecipesAsync();
            return;
        }

        await ExecuteAsync(async () =>
        {
            var list = await _recipeService.SearchRecipesAsync(SearchQuery);
            Recipes.Clear();
            foreach (var r in list) Recipes.Add(r);
        });
    }

    private async Task ToggleFavoriteAsync(RecipeDto recipe)
    {
        recipe.IsFavorite = !recipe.IsFavorite;
        await _recipeService.ToggleFavoriteAsync(recipe.Id, recipe.IsFavorite);
    }
}
