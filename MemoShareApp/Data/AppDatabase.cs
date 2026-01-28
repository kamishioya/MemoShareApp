using SQLite;
using MemoShareApp.Models;

namespace MemoShareApp.Data;

public class AppDatabase
{
    private readonly SQLiteAsyncConnection _database;
    private readonly Task _initializationTask;

    public AppDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _initializationTask = InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<Memo>();
        await _database.CreateTableAsync<MemoShare>();
        
        // サンプルデータの追加（初回のみ）
        var userCount = await _database.Table<User>().CountAsync();
        if (userCount == 0)
        {
            await SeedSampleDataAsync();
        }
    }

    private async Task EnsureInitializedAsync()
    {
        await _initializationTask;
    }

    private async Task SeedSampleDataAsync()
    {
        // サンプルユーザー
        var users = new[]
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
        };

        foreach (var user in users)
        {
            await _database.InsertAsync(user);
        }

        // サンプルメモ
        var memos = new[]
        {
            new Memo
            {
                Id = Guid.NewGuid().ToString(),
                Title = "買い物リスト",
                Content = "牛乳、卵、パン、野菜",
                AuthorId = "user1",
                AuthorName = "テストユーザー1",
                IsShared = true
            },
            new Memo
            {
                Id = Guid.NewGuid().ToString(),
                Title = "会議メモ",
                Content = "次回の会議は来週月曜日",
                AuthorId = "user1",
                AuthorName = "テストユーザー1",
                IsShared = false
            },
            new Memo
            {
                Id = Guid.NewGuid().ToString(),
                Title = "共有メモ",
                Content = "これは共有されたメモです",
                AuthorId = "user2",
                AuthorName = "テストユーザー2",
                IsShared = true
            }
        };

        foreach (var memo in memos)
        {
            await _database.InsertAsync(memo);
        }

        // 共有設定
        await _database.InsertAsync(new MemoShare
        {
            MemoId = memos[2].Id,
            UserId = "user1"
        });
    }

    // User operations
    public async Task<List<User>> GetUsersAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<User>().ToListAsync();
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        await EnsureInitializedAsync();
        return await _database.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        await EnsureInitializedAsync();
        return await _database.Table<User>().Where(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        await EnsureInitializedAsync();
        return await _database.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<int> InsertUserAsync(User user)
    {
        await EnsureInitializedAsync();
        return await _database.InsertAsync(user);
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        await EnsureInitializedAsync();
        return await _database.UpdateAsync(user);
    }

    public async Task<int> DeleteUserAsync(User user)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(user);
    }

    // Memo operations
    public async Task<List<Memo>> GetMemosAsync()
    {
        await EnsureInitializedAsync();
        return await _database.Table<Memo>().ToListAsync();
    }

    public async Task<List<Memo>> GetMemosByAuthorAsync(string authorId)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Memo>().Where(m => m.AuthorId == authorId).ToListAsync();
    }

    public async Task<Memo> GetMemoByIdAsync(string id)
    {
        await EnsureInitializedAsync();
        return await _database.Table<Memo>().Where(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> InsertMemoAsync(Memo memo)
    {
        await EnsureInitializedAsync();
        return await _database.InsertAsync(memo);
    }

    public async Task<int> UpdateMemoAsync(Memo memo)
    {
        await EnsureInitializedAsync();
        return await _database.UpdateAsync(memo);
    }

    public async Task<int> DeleteMemoAsync(Memo memo)
    {
        await EnsureInitializedAsync();
        return await _database.DeleteAsync(memo);
    }

    // MemoShare operations
    public async Task<List<MemoShare>> GetMemoSharesAsync(string memoId)
    {
        await EnsureInitializedAsync();
        return await _database.Table<MemoShare>().Where(ms => ms.MemoId == memoId).ToListAsync();
    }

    public async Task<List<MemoShare>> GetSharedMemosForUserAsync(string userId)
    {
        await EnsureInitializedAsync();
        return await _database.Table<MemoShare>().Where(ms => ms.UserId == userId).ToListAsync();
    }

    public async Task<int> InsertMemoShareAsync(MemoShare memoShare)
    {
        await EnsureInitializedAsync();
        return await _database.InsertAsync(memoShare);
    }

    public async Task<int> DeleteMemoShareAsync(string memoId, string userId)
    {
        await EnsureInitializedAsync();
        return await _database.Table<MemoShare>()
            .DeleteAsync(ms => ms.MemoId == memoId && ms.UserId == userId);
    }
}
