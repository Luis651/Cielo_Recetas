using System.Collections.ObjectModel;
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
        set 
        { 
            SetProperty(ref _recipe, value);
            if (value != null)
            {
                TargetMoldSize = value.BaseMoldSize;
                UpdateScaling();
            }
        }
    }

    // Scaling Logic (HU-001)
    private double _targetMoldSize;
    public double TargetMoldSize
    {
        get => _targetMoldSize;
        set 
        { 
            if (SetProperty(ref _targetMoldSize, value))
            {
                UpdateScaling();
            }
        }
    }

    private double _scalingFactor = 1.0;
    public double ScalingFactor
    {
        get => _scalingFactor;
        private set => SetProperty(ref _scalingFactor, value);
    }

    public ObservableCollection<ScaledIngredient> ScaledIngredients { get; } = new();

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

    private void UpdateScaling()
    {
        if (Recipe == null || Recipe.BaseMoldSize <= 0 || TargetMoldSize <= 0)
        {
            ScalingFactor = 1.0;
        }
        else
        {
            // Factor = (R_new^2) / (R_original^2)
            // Equivalent to (D_new^2) / (D_original^2)
            ScalingFactor = Math.Pow(TargetMoldSize, 2) / Math.Pow(Recipe.BaseMoldSize, 2);
        }

        ScaledIngredients.Clear();
        if (Recipe != null)
        {
            foreach (var ing in Recipe.Ingredients)
            {
                ScaledIngredients.Add(new ScaledIngredient
                {
                    Name = ing.Name,
                    OriginalQuantity = ing.Quantity,
                    ScaledQuantity = ing.Quantity * ScalingFactor,
                    Unit = ing.Unit
                });
            }
        }
    }

    private async Task LoadRecipeAsync()
    {
        await ExecuteAsync(async () =>
        {
            Recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
            if (Recipe is not null) 
            {
                Title = Recipe.Name;
            }
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

public class ScaledIngredient
{
    public string Name { get; set; } = string.Empty;
    public double OriginalQuantity { get; set; }
    public double ScaledQuantity { get; set; }
    public string? Unit { get; set; }
}
