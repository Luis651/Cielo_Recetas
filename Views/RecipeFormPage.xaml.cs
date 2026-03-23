using DulceRecetario.ViewModels;

namespace DulceRecetario.Views;

public partial class RecipeFormPage : ContentPage
{
    public RecipeFormPage(RecipeFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
