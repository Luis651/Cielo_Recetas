using DulceRecetario.ViewModels;

namespace DulceRecetario.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
