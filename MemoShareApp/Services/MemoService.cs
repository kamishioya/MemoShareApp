using MemoShareApp.Models;

namespace MemoShareApp.Services;

public interface IMemoService
{
    /// <summary>
    /// 現在のユーザーのメモ一覧を取得します。
    /// </summary>
    /// <returns>ユーザーのメモ一覧</returns>
    Task<List<Memo>> GetMyMemosAsync();
    
    /// <summary>
    /// 共有されているメモ一覧を取得します。
    /// </summary>
    /// <returns>共有メモ一覧</returns>
    Task<List<Memo>> GetSharedMemosAsync();
    
    /// <summary>
    /// 指定されたIDのメモを取得します。
    /// </summary>
    /// <param name="id">メモID</param>
    /// <returns>メモ、存在しない場合はnull</returns>
    Task<Memo?> GetMemoByIdAsync(string id);
    
    /// <summary>
    /// 新しいメモを作成します。
    /// </summary>
    /// <param name="memo">作成するメモ</param>
    /// <returns>作成されたメモ</returns>
    Task<Memo> CreateMemoAsync(Memo memo);
    
    /// <summary>
    /// 既存のメモを更新します。
    /// </summary>
    /// <param name="memo">更新するメモ</param>
    /// <returns>更新されたメモ</returns>
    Task<Memo> UpdateMemoAsync(Memo memo);
    
    /// <summary>
    /// 指定されたIDのメモを削除します。
    /// </summary>
    /// <param name="id">削除するメモのID</param>
    Task DeleteMemoAsync(string id);
    
    /// <summary>
    /// メモを指定されたユーザーと共有します。
    /// </summary>
    /// <param name="memoId">共有するメモのID</param>
    /// <param name="userId">共有先のユーザーID</param>
    Task ShareMemoAsync(string memoId, string userId);
    
    /// <summary>
    /// メモの共有を解除します。
    /// </summary>
    /// <param name="memoId">対象のメモID</param>
    /// <param name="userId">共有を解除するユーザーID</param>
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

    /// <summary>
    /// サンプルデータを初期化します。
    /// </summary>
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

    /// <summary>
    /// 現在のユーザーのメモ一覧を取得します。
    /// </summary>
    /// <returns>ユーザーのメモ一覧</returns>
    public Task<List<Memo>> GetMyMemosAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return Task.FromResult(new List<Memo>());

        var myMemos = _memos.Where(m => m.AuthorId == currentUser.Id).ToList();
        return Task.FromResult(myMemos);
    }

    /// <summary>
    /// 共有されているメモ一覧を取得します。
    /// </summary>
    /// <returns>共有メモ一覧</returns>
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

    /// <summary>
    /// 指定されたIDのメモを取得します。
    /// </summary>
    /// <param name="id">メモID</param>
    /// <returns>メモ、存在しない場合はnull</returns>
    public Task<Memo?> GetMemoByIdAsync(string id)
    {
        var memo = _memos.FirstOrDefault(m => m.Id == id);
        return Task.FromResult(memo);
    }

    /// <summary>
    /// 新しいメモを作成します。
    /// </summary>
    /// <param name="memo">作成するメモ</param>
    /// <returns>作成されたメモ</returns>
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

    /// <summary>
    /// 既存のメモを更新します。
    /// </summary>
    /// <param name="memo">更新するメモ</param>
    /// <returns>更新されたメモ</returns>
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

    /// <summary>
    /// 指定されたIDのメモを削除します。
    /// </summary>
    /// <param name="id">削除するメモのID</param>
    public Task DeleteMemoAsync(string id)
    {
        var memo = _memos.FirstOrDefault(m => m.Id == id);
        if (memo != null)
        {
            _memos.Remove(memo);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// メモを指定されたユーザーと共有します。
    /// </summary>
    /// <param name="memoId">共有するメモのID</param>
    /// <param name="userId">共有先のユーザーID</param>
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

    /// <summary>
    /// メモの共有を解除します。
    /// </summary>
    /// <param name="memoId">対象のメモID</param>
    /// <param name="userId">共有を解除するユーザーID</param>
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
