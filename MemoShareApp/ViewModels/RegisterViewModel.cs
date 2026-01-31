using System.Windows.Input;
using MemoShareApp.Services;

namespace MemoShareApp.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private string _username = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public RegisterViewModel(IAuthService authService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _serviceProvider = serviceProvider;
        Title = "新規登録";
        RegisterCommand = new Command(async () => await RegisterAsync());
        GoToLoginCommand = new Command(async () => await GoToLoginAsync());
    }

    private async Task RegisterAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || 
                string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "すべての項目を入力してください";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "パスワードが一致しません";
                return;
            }

            var success = await _authService.RegisterAsync(Username, Email, Password);
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
                ErrorMessage = "登録に失敗しました。ユーザー名またはメールアドレスが既に使用されています";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoToLoginAsync()
    {
        var currentPage = Application.Current?.Windows.Count > 0 
            ? Application.Current.Windows[0].Page 
            : null;
        if (currentPage is NavigationPage navigationPage)
        {
            await navigationPage.PopAsync();
        }
    }
}
