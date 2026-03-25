using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DulceRecetario.ViewModels;

/// <summary>
/// ViewModel base con INotifyPropertyChanged y helpers de comandos.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    // ── INotifyPropertyChanged ────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // ── Estado de carga ───────────────────────────────────────────────────

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnIsBusyChanged();
            }
        }
    }

    protected virtual void OnIsBusyChanged() { }

    public bool IsNotBusy => !IsBusy;

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    // ── Mensajes de error ─────────────────────────────────────────────────

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // ── Helper para ejecutar tareas async con manejo de error ─────────────

    protected virtual async Task ExecuteAsync(Func<Task> action, string? errorMessage = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = errorMessage ?? ex.Message;
            // Feedback visual profesional automático
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "Aceptar");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Helper de navegación ──────────────────────────────────────────────

    protected static Task NavigateToAsync(string route, Dictionary<string, object>? parameters = null)
    {
        if (parameters is not null)
            return Shell.Current.GoToAsync(route, parameters);
        return Shell.Current.GoToAsync(route);
    }

    protected static Task GoBackAsync(bool refresh = false) => 
        refresh ? Shell.Current.GoToAsync("..?refresh=true") : Shell.Current.GoToAsync("..");
}
