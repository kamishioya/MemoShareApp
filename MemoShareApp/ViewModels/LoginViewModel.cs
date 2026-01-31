using System.Windows.Input;
using MemoShareApp.Services;
using MemoShareApp.Views;

namespace MemoShareApp.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegisterCommand { get; }

    public LoginViewModel(IAuthService authService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _serviceProvider = serviceProvider;
        Title = "ログイン";
        LoginCommand = new Command(async () => await LoginAsync());
        GoToRegisterCommand = new Command(async () => await GoToRegisterAsync());
    }

    private async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "ユーザー名とパスワードを入力してください";
                return;
            }

            var success = await _authService.LoginAsync(Username, Password);
            if (success)
            {
                if (Application.Current?.Windows.Count > 0)
                {
                    var appShell = _serviceProvider.GetRequiredService<AppShell>();
                    Application.Current.Windows[0].Page = appShell;
                }
            }
            else
            {
                ErrorMessage = "ログインに失敗しました";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoToRegisterAsync()
    {
        var registerPage = _serviceProvider.GetRequiredService<RegisterPage>();
        var currentPage = Application.Current?.Windows.Count > 0 
            ? Application.Current.Windows[0].Page 
            : null;
        if (currentPage is NavigationPage navigationPage)
        {
            await navigationPage.PushAsync(registerPage);
        }
    }
}
