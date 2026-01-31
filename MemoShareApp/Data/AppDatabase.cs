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
    }

    private async Task EnsureInitializedAsync()
    {
        await _initializationTask;
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

    public async Task DeleteMemoAsync(string id)
    {
        await EnsureInitializedAsync();
        var memo = await GetMemoByIdAsync(id);
        if (memo != null)
        {
            await _database.DeleteAsync(memo);
        }
    }

    public async Task SaveMemoAsync(Memo memo)
    {
        await EnsureInitializedAsync();
        var existing = await GetMemoByIdAsync(memo.Id);
        if (existing != null)
        {
            await _database.UpdateAsync(memo);
        }
        else
        {
            await _database.InsertAsync(memo);
        }
    }

    public async Task<List<Memo>> GetSharedMemosAsync(string userId)
    {
        await EnsureInitializedAsync();
        // Get memo IDs shared with this user
        var shares = await _database.Table<MemoShare>().Where(ms => ms.UserId == userId).ToListAsync();
        var sharedMemoIds = shares.Select(s => s.MemoId).ToList();
        
        // Get public memos (IsShared = true) from other users
        var allMemos = await _database.Table<Memo>().ToListAsync();
        return allMemos.Where(m => m.IsShared && m.AuthorId != userId || sharedMemoIds.Contains(m.Id)).ToList();
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
