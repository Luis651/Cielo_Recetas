using System.Windows.Input;
using DulceRecetario.Services;
using DulceRecetario.Views;

namespace DulceRecetario.ViewModels;

public class AuthViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }
    public ICommand GoToRegisterCommand { get; }
    public ICommand GoBackAsyncCommand { get; }
    public ICommand BackToLoginCommand { get; }

    public AuthViewModel(AuthService authService)
    {
        _authService = authService;

        LoginCommand = new Command(async () => await LoginAsync());
        RegisterCommand = new Command(async () => await RegisterAsync());
        GoToRegisterCommand = new Command(async () => await NavigateToAsync(nameof(RegisterPage)));
        GoBackAsyncCommand = new Command(async () => await GoBackAsync());
        BackToLoginCommand = new Command(async () => await GoBackAsync());
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            return;

        await ExecuteAsync(async () =>
        {
            var session = await _authService.SignInAsync(Email, Password);
            if (session != null)
            {
                await Shell.Current.GoToAsync($"///{nameof(RecipeListPage)}");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Credenciales inválidas", "OK");
            }
        });
    }

    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            return;

        await ExecuteAsync(async () =>
        {
            var session = await _authService.SignUpAsync(Email, Password);
            if (session != null)
            {
                await Shell.Current.DisplayAlert("Éxito", "Usuario registrado. Por favor verifica tu email.", "OK");
                await GoBackAsync();
            }
        });
    }
}
