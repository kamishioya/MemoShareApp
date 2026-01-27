using MemoShareApp.Models;

namespace MemoShareApp.Services;

public interface IMemoService
{
    Task<List<Memo>> GetMyMemosAsync();
    Task<List<Memo>> GetSharedMemosAsync();
    Task<Memo?> GetMemoByIdAsync(string id);
    Task<Memo> CreateMemoAsync(Memo memo);
    Task<Memo> UpdateMemoAsync(Memo memo);
    Task DeleteMemoAsync(string id);
    Task ShareMemoAsync(string memoId, string userId);
    Task UnshareMemoAsync(string memoId, string userId);
}

public class MemoService : IMemoService
{
    private readonly List<Memo> _memos = new();
    private readonly IAuthService _authService;

    public MemoService(IAuthService authService)
    {
        _authService = authService;
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        _memos.AddRange(new[]
        {
            new Memo
            {
                Title = "買い物リスト",
                Content = "牛乳、卵、パン、野菜",
                AuthorId = "user1",
                AuthorName = "テストユーザー1",
                IsShared = true
            },
            new Memo
            {
                Title = "会議メモ",
                Content = "次回の会議は来週月曜日",
                AuthorId = "user1",
                AuthorName = "テストユーザー1"
            },
            new Memo
            {
                Title = "共有メモ",
                Content = "これは共有されたメモです",
                AuthorId = "user2",
                AuthorName = "テストユーザー2",
                IsShared = true,
                SharedWithUserIds = new List<string> { "user1" }
            }
        });
    }

    public Task<List<Memo>> GetMyMemosAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return Task.FromResult(new List<Memo>());

        var myMemos = _memos.Where(m => m.AuthorId == currentUser.Id).ToList();
        return Task.FromResult(myMemos);
    }

    public Task<List<Memo>> GetSharedMemosAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return Task.FromResult(new List<Memo>());

        var sharedMemos = _memos.Where(m => 
            m.IsShared && 
            (m.SharedWithUserIds.Contains(currentUser.Id) || m.AuthorId == currentUser.Id))
            .ToList();
        return Task.FromResult(sharedMemos);
    }

    public Task<Memo?> GetMemoByIdAsync(string id)
    {
        var memo = _memos.FirstOrDefault(m => m.Id == id);
        return Task.FromResult(memo);
    }

    public Task<Memo> CreateMemoAsync(Memo memo)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser != null)
        {
            memo.AuthorId = currentUser.Id;
            memo.AuthorName = currentUser.DisplayName;
        }
        memo.CreatedAt = DateTime.Now;
        memo.UpdatedAt = DateTime.Now;
        _memos.Add(memo);
        return Task.FromResult(memo);
    }

    public Task<Memo> UpdateMemoAsync(Memo memo)
    {
        var existing = _memos.FirstOrDefault(m => m.Id == memo.Id);
        if (existing != null)
        {
            existing.Title = memo.Title;
            existing.Content = memo.Content;
            existing.IsShared = memo.IsShared;
            existing.UpdatedAt = DateTime.Now;
        }
        return Task.FromResult(memo);
    }

    public Task DeleteMemoAsync(string id)
    {
        var memo = _memos.FirstOrDefault(m => m.Id == id);
        if (memo != null)
        {
            _memos.Remove(memo);
        }
        return Task.CompletedTask;
    }

    public Task ShareMemoAsync(string memoId, string userId)
    {
        var memo = _memos.FirstOrDefault(m => m.Id == memoId);
        if (memo != null && !memo.SharedWithUserIds.Contains(userId))
        {
            memo.SharedWithUserIds.Add(userId);
            memo.IsShared = true;
        }
        return Task.CompletedTask;
    }

    public Task UnshareMemoAsync(string memoId, string userId)
    {
        var memo = _memos.FirstOrDefault(m => m.Id == memoId);
        if (memo != null)
        {
            memo.SharedWithUserIds.Remove(userId);
            if (memo.SharedWithUserIds.Count == 0)
            {
                memo.IsShared = false;
            }
        }
        return Task.CompletedTask;
    }
}
