using MemoShareApp.Models;

namespace MemoShareApp.Services;

public interface IAuthService
{
    /// <summary>
    /// 現在ログインしているユーザーを取得します。
    /// </summary>
    User? CurrentUser { get; }
    
    /// <summary>
    /// ユーザーがログインしているかどうかを取得します。
    /// </summary>
    bool IsLoggedIn { get; }
    
    /// <summary>
    /// ユーザー認証を行います。
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="password">パスワード</param>
    /// <returns>ログイン成功の場合true、失敗の場合false</returns>
    Task<bool> LoginAsync(string username, string password);
    
    /// <summary>
    /// ログアウトします。
    /// </summary>
    Task LogoutAsync();
    
    /// <summary>
    /// 新しいユーザーを登録します。
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="email">メールアドレス</param>
    /// <param name="password">パスワード</param>
    /// <returns>登録成功の場合true、失敗の場合false</returns>
    Task<bool> RegisterAsync(string username, string email, string password);
    
    /// <summary>
    /// 全ユーザーの一覧を取得します（現在のユーザーを除く）。
    /// </summary>
    /// <returns>ユーザー一覧</returns>
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

    /// <summary>
    /// サンプルユーザーを初期化します。
    /// </summary>
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

    /// <summary>
    /// ユーザー認証を行います。
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="password">パスワード</param>
    /// <returns>ログイン成功の場合true、失敗の場合false</returns>
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

    /// <summary>
    /// ログアウトします。
    /// </summary>
    public Task LogoutAsync()
    {
        CurrentUser = null;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 新しいユーザーを登録します。
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="email">メールアドレス</param>
    /// <param name="password">パスワード</param>
    /// <returns>登録成功の場合true、失敗の場合false</returns>
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

    /// <summary>
    /// 全ユーザーの一覧を取得します（現在のユーザーを除く）。
    /// </summary>
    /// <returns>ユーザー一覧</returns>
    public Task<List<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users.Where(u => u.Id != CurrentUser?.Id).ToList());
    }
}
