using MemoShareApp.Models;
using MemoShareApp.Data;

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
    private readonly AppDatabase _database;
    private readonly IAuthService _authService;

    public MemoService(AppDatabase database, IAuthService authService)
    {
        _database = database;
        _authService = authService;
    }

    /// <summary>
    /// 現在のユーザーのメモ一覧を取得します。
    /// </summary>
    /// <returns>ユーザーのメモ一覧</returns>
    public async Task<List<Memo>> GetMyMemosAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return new List<Memo>();

        return await _database.GetMemosByAuthorAsync(currentUser.Id);
    }

    /// <summary>
    /// 共有されているメモ一覧を取得します。
    /// </summary>
    /// <returns>共有メモ一覧</returns>
    public async Task<List<Memo>> GetSharedMemosAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return new List<Memo>();

        // 自分が作成した共有メモ
        var mySharedMemos = await _database.GetMemosByAuthorAsync(currentUser.Id);
        mySharedMemos = mySharedMemos.Where(m => m.IsShared).ToList();

        // 自分に共有されたメモ
        var sharedWithMe = await _database.GetSharedMemosForUserAsync(currentUser.Id);
        var sharedMemoIds = sharedWithMe.Select(s => s.MemoId).ToList();
        
        var allMemos = await _database.GetMemosAsync();
        var othersSharedMemos = allMemos.Where(m => sharedMemoIds.Contains(m.Id)).ToList();

        // 結合して重複を除去
        var result = mySharedMemos.Concat(othersSharedMemos)
            .GroupBy(m => m.Id)
            .Select(g => g.First())
            .ToList();

        // SharedWithUserIdsを設定
        foreach (var memo in result)
        {
            var shares = await _database.GetMemoSharesAsync(memo.Id);
            memo.SharedWithUserIds = shares.Select(s => s.UserId).ToList();
        }

        return result;
    }

    /// <summary>
    /// 指定されたIDのメモを取得します。
    /// </summary>
    /// <param name="id">メモID</param>
    /// <returns>メモ、存在しない場合はnull</returns>
    public async Task<Memo?> GetMemoByIdAsync(string id)
    {
        var memo = await _database.GetMemoByIdAsync(id);
        if (memo != null)
        {
            var shares = await _database.GetMemoSharesAsync(memo.Id);
            memo.SharedWithUserIds = shares.Select(s => s.UserId).ToList();
        }
        return memo;
    }

    /// <summary>
    /// 新しいメモを作成します。
    /// </summary>
    /// <param name="memo">作成するメモ</param>
    /// <returns>作成されたメモ</returns>
    public async Task<Memo> CreateMemoAsync(Memo memo)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser != null)
        {
            memo.AuthorId = currentUser.Id;
            memo.AuthorName = currentUser.DisplayName;
        }
        memo.CreatedAt = DateTime.Now;
        memo.UpdatedAt = DateTime.Now;
        
        await _database.InsertMemoAsync(memo);
        return memo;
    }

    /// <summary>
    /// 既存のメモを更新します。
    /// </summary>
    /// <param name="memo">更新するメモ</param>
    /// <returns>更新されたメモ</returns>
    public async Task<Memo> UpdateMemoAsync(Memo memo)
    {
        var existing = await _database.GetMemoByIdAsync(memo.Id);
        if (existing != null)
        {
            existing.Title = memo.Title;
            existing.Content = memo.Content;
            existing.IsShared = memo.IsShared;
            existing.UpdatedAt = DateTime.Now;
            
            await _database.UpdateMemoAsync(existing);
        }
        return memo;
    }

    /// <summary>
    /// 指定されたIDのメモを削除します。
    /// </summary>
    /// <param name="id">削除するメモのID</param>
    public async Task DeleteMemoAsync(string id)
    {
        var memo = await _database.GetMemoByIdAsync(id);
        if (memo != null)
        {
            await _database.DeleteMemoAsync(memo);
        }
    }

    /// <summary>
    /// メモを指定されたユーザーと共有します。
    /// </summary>
    /// <param name="memoId">共有するメモのID</param>
    /// <param name="userId">共有先のユーザーID</param>
    public async Task ShareMemoAsync(string memoId, string userId)
    {
        var memo = await _database.GetMemoByIdAsync(memoId);
        if (memo != null)
        {
            var shares = await _database.GetMemoSharesAsync(memoId);
            if (!shares.Any(s => s.UserId == userId))
            {
                await _database.InsertMemoShareAsync(new MemoShare
                {
                    MemoId = memoId,
                    UserId = userId
                });
                
                memo.IsShared = true;
                await _database.UpdateMemoAsync(memo);
            }
        }
    }

    /// <summary>
    /// メモの共有を解除します。
    /// </summary>
    /// <param name="memoId">対象のメモID</param>
    /// <param name="userId">共有を解除するユーザーID</param>
    public async Task UnshareMemoAsync(string memoId, string userId)
    {
        await _database.DeleteMemoShareAsync(memoId, userId);
        
        var shares = await _database.GetMemoSharesAsync(memoId);
        if (shares.Count == 0)
        {
            var memo = await _database.GetMemoByIdAsync(memoId);
            if (memo != null)
            {
                memo.IsShared = false;
                await _database.UpdateMemoAsync(memo);
            }
        }
    }
}

