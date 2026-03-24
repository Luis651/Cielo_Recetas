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
        set 
        { 
            _recipeId = value; 
            OnPropertyChanged(nameof(IsEditMode)); 
            OnPropertyChanged(nameof(FormTitle)); 
            if (value != Guid.Empty) _ = LoadRecipeAsync(); 
        }
    }

    // Campos del formulario
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        set 
        {
            if (SetProperty(ref _name, value))
            {
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    private string _category = "Tortas";
    public string Category { get => _category; set => SetProperty(ref _category, value); }

    private string _difficulty = "Fácil";
    public string Difficulty { get => _difficulty; set => SetProperty(ref _difficulty, value); }

    private int _prepTimeMinutes = 30;
    public int PrepTimeMinutes { get => _prepTimeMinutes; set => SetProperty(ref _prepTimeMinutes, value); }

    private int _servings = 1;
    public int Servings { get => _servings; set => SetProperty(ref _servings, value); }

    private double _baseMoldSize = 20.0;
    public double BaseMoldSize { get => _baseMoldSize; set => SetProperty(ref _baseMoldSize, value); }

    // Ingredientes (HU-001)
    public ObservableCollection<IngredientDto> Ingredients { get; } = new();

    // Pasos (HU-001)
    public ObservableCollection<RecipeStepDto> Steps { get; } = new();

    public ObservableCollection<string> Categories { get; } = new()
    {
        "Tortas", "Bizcochos", "Galletas", "Helados", "Tartas", "Bombones", "Mousses", "Bebidas dulces", "Otros"
    };

    public ObservableCollection<string> DifficultyLevels { get; } = new()
    {
        "Fácil", "Intermedia", "Difícil"
    };

    public ObservableCollection<string> Units { get; } = new()
    {
        "g", "kg", "ml", "L", "Taza", "Cucharada", "Cucharadita", "Unidad", "Pizca"
    };

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand AddIngredientCommand { get; }
    public ICommand RemoveIngredientCommand { get; }
    public ICommand AddStepCommand { get; }
    public ICommand RemoveStepCommand { get; }

    public RecipeFormViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Title = FormTitle;

        SaveCommand = new Command(async () => await SaveRecipeAsync(), () => !string.IsNullOrWhiteSpace(Name) && !IsBusy);
        CancelCommand = new Command(async () => await GoBackAsync());
        
        AddIngredientCommand = new Command(() => 
        {
            Ingredients.Add(new IngredientDto { OrderIndex = Ingredients.Count });
        });

        RemoveIngredientCommand = new Command<IngredientDto>((ingredient) => 
        {
            if (ingredient != null)
                Ingredients.Remove(ingredient);
        });

        AddStepCommand = new Command(() => 
        {
            Steps.Add(new RecipeStepDto { Order = Steps.Count + 1 });
        });

        RemoveStepCommand = new Command<RecipeStepDto>((step) => 
        {
            if (step != null)
            {
                Steps.Remove(step);
                // Re-order steps
                for (int i = 0; i < Steps.Count; i++)
                {
                    Steps[i].Order = i + 1;
                }
            }
        });

        // Add one empty ingredient by default if creating
        if (!IsEditMode && Ingredients.Count == 0)
        {
            Ingredients.Add(new IngredientDto { OrderIndex = 0, Unit = "g" });
        }

        // Add one empty step by default if creating (optional, but good for UX)
        if (!IsEditMode && Steps.Count == 0)
        {
            Steps.Add(new RecipeStepDto { Order = 1 });
        }
    }

    protected override void OnIsBusyChanged()
    {
        ((Command)SaveCommand).ChangeCanExecute();
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
            BaseMoldSize = recipe.BaseMoldSize;

            Ingredients.Clear();
            foreach (var ing in recipe.Ingredients)
            {
                Ingredients.Add(ing);
            }

            Steps.Clear();
            foreach (var step in recipe.Steps)
            {
                Steps.Add(step);
            }
        });
        
        // Ensure command re-evaluates after loading
        ((Command)SaveCommand).ChangeCanExecute();
    }

    private async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Ups", "El nombre de la receta es obligatorio", "Entendido");
            return;
        }

        await ExecuteAsync(async () =>
        {
            // Filter out empty ingredients
            var validIngredients = Ingredients
                .Where(i => !string.IsNullOrWhiteSpace(i.Name))
                .ToList();

            // Filter out empty steps
            var validSteps = Steps
                .Where(s => !string.IsNullOrWhiteSpace(s.Description))
                .ToList();

            var dto = new RecipeDto
            {
                Id = IsEditMode ? RecipeId : Guid.NewGuid(),
                Name = Name?.Trim() ?? "",
                Description = Description?.Trim() ?? "",
                Category = Category,
                Difficulty = Difficulty,
                PrepTimeMinutes = PrepTimeMinutes,
                Servings = Servings,
                BaseMoldSize = BaseMoldSize,
                Ingredients = validIngredients,
                Steps = validSteps
            };

            bool success;
            if (IsEditMode)
                success = await _recipeService.UpdateRecipeAsync(dto) != null;
            else
                success = await _recipeService.CreateRecipeAsync(dto) != null;

            if (success)
            {
                await Shell.Current.DisplayAlert("¡Éxito!", "Receta guardada correctamente 🍰", "Genial");
                await GoBackAsync();
            }
            else
            {
                throw new Exception("No se pudo guardar la receta en el servidor.");
            }
        });
    }
}
