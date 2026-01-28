using MemoShareApp.Models;
using MemoShareApp.Data;

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
    private readonly AppDatabase _database;
    
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public AuthService(AppDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// ユーザー認証を行います。
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="password">パスワード</param>
    /// <returns>ログイン成功の場合true、失敗の場合false</returns>
    public async Task<bool> LoginAsync(string username, string password)
    {
        // 簡易的なログイン（実際のアプリでは認証サーバーを使用）
        var user = await _database.GetUserByUsernameAsync(username);
        if (user != null)
        {
            CurrentUser = user;
            return true;
        }
        return false;
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
    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        var existingUser = await _database.GetUserByUsernameAsync(username);
        var existingEmail = await _database.GetUserByEmailAsync(email);
        
        if (existingUser != null || existingEmail != null)
        {
            return false;
        }

        var newUser = new User
        {
            Username = username,
            Email = email,
            DisplayName = username
        };
        
        await _database.InsertUserAsync(newUser);
        CurrentUser = newUser;
        return true;
    }

    /// <summary>
    /// 全ユーザーの一覧を取得します（現在のユーザーを除く）。
    /// </summary>
    /// <returns>ユーザー一覧</returns>
    public async Task<List<User>> GetAllUsersAsync()
    {
        var allUsers = await _database.GetUsersAsync();
        return allUsers.Where(u => u.Id != CurrentUser?.Id).ToList();
    }
}

