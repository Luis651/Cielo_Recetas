using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
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

    private string? _imageUrl;
    public string? ImageUrl { get => _imageUrl; set => SetProperty(ref _imageUrl, value); }

    private ImageSource? _selectedImageSource;
    public ImageSource? SelectedImageSource { get => _selectedImageSource; set => SetProperty(ref _selectedImageSource, value); }

    private Stream? _selectedImageStream;

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
    public ICommand SelectImageCommand { get; }
    public ICommand TakePhotoCommand { get; }

    public RecipeFormViewModel(RecipeService recipeService)
    {
        _recipeService = recipeService;
        Title = FormTitle;

        SaveCommand = new Command(async () => await SaveRecipeAsync(), () => !string.IsNullOrWhiteSpace(Name) && !IsBusy);
        CancelCommand = new Command(async () => await GoBackAsync());
        SelectImageCommand = new Command(async () => await SelectImageAsync());
        TakePhotoCommand = new Command(async () => await TakePhotoAsync());
        
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

    private async Task SelectImageAsync()
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;
                
                _selectedImageStream = memStream;
                SelectedImageSource = ImageSource.FromStream(() => new MemoryStream(memStream.ToArray()));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo seleccionar la imagen: " + ex.Message, "OK");
        }
    }

    private async Task TakePhotoAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (status != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permiso", "Se requiere permiso de cámara para tomar fotos.", "OK");
                return;
            }

            if (MediaPicker.Default.IsCaptureSupported)
            {
                var result = await MediaPicker.Default.CapturePhotoAsync();
                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var memStream = new MemoryStream();
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;
                    
                    _selectedImageStream = memStream;
                    SelectedImageSource = ImageSource.FromStream(() => new MemoryStream(memStream.ToArray()));
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo tomar la foto: " + ex.Message, "OK");
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
            ImageUrl = recipe.ImageUrl;

            if (!string.IsNullOrEmpty(ImageUrl))
            {
                SelectedImageSource = ImageSource.FromUri(new Uri(ImageUrl));
            }

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
            // 1. Subir imagen si se seleccionó una nueva
            if (_selectedImageStream != null)
            {
                IsBusy = true;
                _selectedImageStream.Position = 0; // Asegurar que estamos al inicio
                var fileName = $"recipe_{Guid.NewGuid()}.jpg";
                var uploadedUrl = await _recipeService.UploadImageAsync(_selectedImageStream, fileName);
                if (uploadedUrl != null)
                {
                    ImageUrl = uploadedUrl;
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo subir la imagen a Supabase. Verifica la conexión y los permisos del bucket 'recetas'.", "Aceptar");
                }
                IsBusy = false;
            }

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
                ImageUrl = ImageUrl,
                Ingredients = validIngredients,
                Steps = validSteps
            };

            bool success;
            if (IsEditMode)
            {
                // Preserve favorite status if edit
                var existing = await _recipeService.GetRecipeByIdAsync(RecipeId);
                if (existing != null) dto.IsFavorite = existing.IsFavorite;
                success = await _recipeService.UpdateRecipeAsync(dto) != null;
            }
            else
            {
                success = await _recipeService.CreateRecipeAsync(dto) != null;
            }

            if (success)
            {
                await Shell.Current.DisplayAlert("¡Éxito!", "Receta guardada correctamente 🍰", "Genial");
                await GoBackAsync(refresh: true);
            }
            else
            {
                throw new Exception("No se pudo guardar la receta en el servidor.");
            }
        });
    }
}
