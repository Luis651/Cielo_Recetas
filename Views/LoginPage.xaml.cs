using DulceRecetario.ViewModels;

namespace DulceRecetario.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
