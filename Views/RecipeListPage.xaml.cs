using DulceRecetario.ViewModels;

namespace DulceRecetario.Views;

public partial class RecipeListPage : ContentPage
{
    private readonly RecipeListViewModel _viewModel;

    public RecipeListPage(RecipeListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRecipesAsync();
    }
}
