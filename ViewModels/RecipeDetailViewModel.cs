using System.Windows.Input;
using DulceRecetario.DTOs;
using DulceRecetario.Services;

namespace DulceRecetario.ViewModels;

[QueryProperty(nameof(RecipeId), "RecipeId")]
public class RecipeDetailViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;

    private Guid _recipeId;
    public Guid RecipeId
    {
        get => _recipeId;
        set { _recipeId = value; _ = LoadRecipeAsync(); }
    }

    private RecipeDto? _recipe;
    public RecipeDto? Recipe
    {
        get => _recipe;
        set => SetProperty(ref _recipe, value);
    }

    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand GoBackCommand { get; }

    public RecipeDetailViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Title = "Detalle";

        EditCommand = new Command(async () =>
            await NavigateToAsync("RecipeFormPage",
                new Dictionary<string, object> { ["RecipeId"] = RecipeId }));

        DeleteCommand = new Command(async () => await DeleteRecipeAsync());
        ToggleFavoriteCommand = new Command(async () => await ToggleFavoriteAsync());
        GoBackCommand = new Command(async () => await GoBackAsync());
    }

    private async Task LoadRecipeAsync()
    {
        await ExecuteAsync(async () =>
        {
            Recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
            if (Recipe is not null) Title = Recipe.Name;
        });
    }

    private async Task DeleteRecipeAsync()
    {
        if (Recipe is null) return;
        await ExecuteAsync(async () =>
        {
            await _recipeService.DeleteRecipeAsync(Recipe.Id);
            await GoBackAsync();
        });
    }

    private async Task ToggleFavoriteAsync()
    {
        if (Recipe is null) return;
        Recipe.IsFavorite = !Recipe.IsFavorite;
        OnPropertyChanged(nameof(Recipe));
        await _recipeService.ToggleFavoriteAsync(Recipe.Id, Recipe.IsFavorite);
    }
}
