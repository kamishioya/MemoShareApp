using System.Windows.Input;
using MemoShareApp.Services;

namespace MemoShareApp.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
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

    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
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
                Application.Current!.MainPage = new AppShell();
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
        await Shell.Current.GoToAsync("..");
    }
}
