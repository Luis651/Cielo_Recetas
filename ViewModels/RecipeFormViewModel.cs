using System.Collections.ObjectModel;
using System.Windows.Input;
using DulceRecetario.DTOs;
using DulceRecetario.Services;

namespace DulceRecetario.ViewModels;

[QueryProperty(nameof(RecipeId), "RecipeId")]
public class RecipeFormViewModel : BaseViewModel
{
    private readonly RecipeService _recipeService;

    // Modo edición vs creación
    public bool IsEditMode => RecipeId != Guid.Empty;
    public string FormTitle => IsEditMode ? "Editar Receta" : "Nueva Receta";

    private Guid _recipeId;
    public Guid RecipeId
    {
        get => _recipeId;
        set { _recipeId = value; OnPropertyChanged(nameof(IsEditMode)); OnPropertyChanged(nameof(FormTitle)); if (value != Guid.Empty) _ = LoadRecipeAsync(); }
    }

    // Campos del formulario
    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    private string _category = string.Empty;
    public string Category { get => _category; set => SetProperty(ref _category, value); }

    private string _difficulty = "Fácil";
    public string Difficulty { get => _difficulty; set => SetProperty(ref _difficulty, value); }

    private int _prepTimeMinutes;
    public int PrepTimeMinutes { get => _prepTimeMinutes; set => SetProperty(ref _prepTimeMinutes, value); }

    private int _servings = 4;
    public int Servings { get => _servings; set => SetProperty(ref _servings, value); }

    public ObservableCollection<string> Categories { get; } = new()
    {
        "Tortas", "Galletas", "Helados", "Tartas", "Bombones", "Mousses", "Bebidas dulces", "Otros"
    };

    public ObservableCollection<string> DifficultyLevels { get; } = new()
    {
        "Fácil", "Intermedia", "Difícil"
    };

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public RecipeFormViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Title = FormTitle;

        SaveCommand = new Command(async () => await SaveRecipeAsync(), () => !string.IsNullOrWhiteSpace(Name));
        CancelCommand = new Command(async () => await GoBackAsync());
    }

    private async Task LoadRecipeAsync()
    {
        await ExecuteAsync(async () =>
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
            if (recipe is null) return;

            Name = recipe.Name;
            Description = recipe.Description ?? string.Empty;
            Category = recipe.Category ?? string.Empty;
            Difficulty = recipe.Difficulty;
            PrepTimeMinutes = recipe.PrepTimeMinutes;
            Servings = recipe.Servings;
        });
    }

    private async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        await ExecuteAsync(async () =>
        {
            var dto = new RecipeDto
            {
                Id = IsEditMode ? RecipeId : Guid.NewGuid(),
                Name = Name.Trim(),
                Description = Description.Trim(),
                Category = Category,
                Difficulty = Difficulty,
                PrepTimeMinutes = PrepTimeMinutes,
                Servings = Servings
            };

            if (IsEditMode)
                await _recipeService.UpdateRecipeAsync(dto);
            else
                await _recipeService.CreateRecipeAsync(dto);

            await GoBackAsync();
        });
    }
}
