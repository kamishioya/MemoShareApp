using MemoShareApp.Models;

namespace MemoShareApp.Services;

public interface IAuthService
{
    User? CurrentUser { get; }
    bool IsLoggedIn { get; }
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<bool> RegisterAsync(string username, string email, string password);
    Task<List<User>> GetAllUsersAsync();
}

public class AuthService : IAuthService
{
    private readonly List<User> _users = new();
    
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public AuthService()
    {
        InitializeSampleUsers();
    }

    private void InitializeSampleUsers()
    {
        _users.AddRange(new[]
        {
            new User
            {
                Id = "user1",
                Username = "test1",
                Email = "test1@example.com",
                DisplayName = "テストユーザー1"
            },
            new User
            {
                Id = "user2",
                Username = "test2",
                Email = "test2@example.com",
                DisplayName = "テストユーザー2"
            }
        });
    }

    public Task<bool> LoginAsync(string username, string password)
    {
        // 簡易的なログイン（実際のアプリでは認証サーバーを使用）
        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            CurrentUser = user;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        return Task.CompletedTask;
    }

    public Task<bool> RegisterAsync(string username, string email, string password)
    {
        if (_users.Any(u => u.Username == username || u.Email == email))
        {
            return Task.FromResult(false);
        }

        var newUser = new User
        {
            Username = username,
            Email = email,
            DisplayName = username
        };
        _users.Add(newUser);
        CurrentUser = newUser;
        return Task.FromResult(true);
    }

    public Task<List<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users.Where(u => u.Id != CurrentUser?.Id).ToList());
    }
}
