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

    private bool _isLoading;

    public async Task LoadRecipesAsync()
    {
        if (_isLoading) return;
        _isLoading = true;

        try
        {
            IsRefreshing = true;
            var list = await _recipeService.GetAllRecipesAsync();
            Recipes.Clear();
            foreach (var r in list)
                Recipes.Add(r);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudieron cargar las recetas: " + ex.Message, "Aceptar");
        }
        finally
        {
            IsRefreshing = false;
            _isLoading = false;
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadRecipesAsync();
            return;
        }

        try
        {
            var list = await _recipeService.SearchRecipesAsync(SearchQuery);
            Recipes.Clear();
            foreach (var r in list)
                Recipes.Add(r);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Error en búsqueda: " + ex.Message, "Aceptar");
        }
    }

    private async Task ToggleFavoriteAsync(RecipeDto recipe)
    {
        recipe.IsFavorite = !recipe.IsFavorite;
        await _recipeService.ToggleFavoriteAsync(recipe.Id, recipe.IsFavorite);
    }
}
