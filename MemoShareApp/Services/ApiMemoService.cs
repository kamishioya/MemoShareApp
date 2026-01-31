using MemoShareApp.Models;
using MemoShareApp.Models.Api;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace MemoShareApp.Services;

/// <summary>
/// API経由でメモを管理するサービス（API専用、ローカルDB不使用）
/// </summary>
public class ApiMemoService : IMemoService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public ApiMemoService(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
        // BaseAddressはMauiProgram.csで設定済み
    }

    private void SetAuthToken()
    {
        // 本番環境ではSecureStorageからトークンを取得
        // var token = await SecureStorage.GetAsync("auth_token");
        var token = _authService.CurrentUser?.Id; // 仮実装
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    /// <summary>
    /// 現在のユーザーのメモ一覧を取得
    /// </summary>
    public async Task<List<Memo>> GetMyMemosAsync()
    {
        System.Diagnostics.Debug.WriteLine("[GetMyMemosAsync] Starting...");

        try
        {
            SetAuthToken();
            System.Diagnostics.Debug.WriteLine("[GetMyMemosAsync] Fetching from API...");
            var apiMemos = await _httpClient.GetFromJsonAsync<List<ApiMemoDto>>("memos/my");
            
            if (apiMemos != null)
            {
                System.Diagnostics.Debug.WriteLine($"[GetMyMemosAsync] Got {apiMemos.Count} memos from API");
                return apiMemos.Select(ConvertFromApiDto).ToList();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw new InvalidOperationException($"メモの取得に失敗しました: {ex.Message}", ex);
        }

        return new List<Memo>();
    }

    /// <summary>
    /// 共有されているメモ一覧を取得
    /// </summary>
    public async Task<List<Memo>> GetSharedMemosAsync()
    {
        try
        {
            SetAuthToken();
            System.Diagnostics.Debug.WriteLine("[GetSharedMemosAsync] Fetching from API...");
            var apiMemos = await _httpClient.GetFromJsonAsync<List<ApiMemoDto>>("memos/shared");
            
            if (apiMemos != null)
            {
                System.Diagnostics.Debug.WriteLine($"[GetSharedMemosAsync] Got {apiMemos.Count} memos from API");
                return apiMemos.Select(ConvertFromApiDto).ToList();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw new InvalidOperationException($"共有メモの取得に失敗しました: {ex.Message}", ex);
        }

        return new List<Memo>();
    }

    /// <summary>
    /// 指定されたIDのメモを取得
    /// </summary>
    public async Task<Memo?> GetMemoByIdAsync(string id)
    {
        try
        {
            SetAuthToken();
            System.Diagnostics.Debug.WriteLine($"[GetMemoByIdAsync] Fetching memo {id} from API...");
            var apiMemo = await _httpClient.GetFromJsonAsync<ApiMemoDto>($"memos/{id}");
            
            if (apiMemo != null)
            {
                return ConvertFromApiDto(apiMemo);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw new InvalidOperationException($"メモの取得に失敗しました: {ex.Message}", ex);
        }

        return null;
    }

    /// <summary>
    /// 新しいメモを作成
    /// </summary>
    public async Task<Memo> CreateMemoAsync(Memo memo)
    {
        System.Diagnostics.Debug.WriteLine("[CreateMemoAsync] Starting...");

        try
        {
            SetAuthToken();
            var request = new CreateMemoRequest
            {
                Title = memo.Title,
                Content = memo.Content,
                IsShared = memo.IsShared
            };

            System.Diagnostics.Debug.WriteLine("[CreateMemoAsync] Posting to API...");
            var response = await _httpClient.PostAsJsonAsync("memos", request);
            response.EnsureSuccessStatusCode();

            var apiMemo = await response.Content.ReadFromJsonAsync<ApiMemoDto>();
            if (apiMemo != null)
            {
                var createdMemo = ConvertFromApiDto(apiMemo);
                System.Diagnostics.Debug.WriteLine($"[CreateMemoAsync] Created via API with Id: {createdMemo.Id}");
                return createdMemo;
            }

            throw new InvalidOperationException("APIからの応答が空です");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CreateMemoAsync API Error] {ex.Message}");
            throw new InvalidOperationException($"メモの作成に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 既存のメモを更新
    /// </summary>
    public async Task<Memo> UpdateMemoAsync(Memo memo)
    {
        try
        {
            SetAuthToken();
            var request = new UpdateMemoRequest
            {
                Title = memo.Title,
                Content = memo.Content,
                IsShared = memo.IsShared
            };

            System.Diagnostics.Debug.WriteLine($"[UpdateMemoAsync] Updating memo {memo.Id} via API...");
            var response = await _httpClient.PutAsJsonAsync($"memos/{memo.Id}", request);
            response.EnsureSuccessStatusCode();

            var apiMemo = await response.Content.ReadFromJsonAsync<ApiMemoDto>();
            if (apiMemo != null)
            {
                var updatedMemo = ConvertFromApiDto(apiMemo);
                System.Diagnostics.Debug.WriteLine($"[UpdateMemoAsync] Updated via API");
                return updatedMemo;
            }

            throw new InvalidOperationException("APIからの応答が空です");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw new InvalidOperationException($"メモの更新に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 指定されたIDのメモを削除
    /// </summary>
    public async Task DeleteMemoAsync(string id)
    {
        try
        {
            SetAuthToken();
            System.Diagnostics.Debug.WriteLine($"[DeleteMemoAsync] Deleting memo {id} via API...");
            var response = await _httpClient.DeleteAsync($"memos/{id}");
            response.EnsureSuccessStatusCode();
            System.Diagnostics.Debug.WriteLine($"[DeleteMemoAsync] Deleted successfully");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw new InvalidOperationException($"メモの削除に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// メモを指定されたユーザーと共有
    /// </summary>
    public async Task ShareMemoAsync(string memoId, string userId)
    {
        try
        {
            SetAuthToken();
            var request = new ShareMemoRequest
            {
                SharedWithUserId = userId
            };

            System.Diagnostics.Debug.WriteLine($"[ShareMemoAsync] Sharing memo {memoId} with user {userId}...");
            var response = await _httpClient.PostAsJsonAsync($"memos/{memoId}/share", request);
            response.EnsureSuccessStatusCode();
            System.Diagnostics.Debug.WriteLine($"[ShareMemoAsync] Shared successfully");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw new InvalidOperationException($"メモの共有に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// メモの共有を解除
    /// </summary>
    public async Task UnshareMemoAsync(string memoId, string userId)
    {
        try
        {
            SetAuthToken();
            System.Diagnostics.Debug.WriteLine($"[UnshareMemoAsync] Unsharing memo {memoId} from user {userId}...");
            var response = await _httpClient.DeleteAsync($"memos/{memoId}/share/{userId}");
            response.EnsureSuccessStatusCode();
            System.Diagnostics.Debug.WriteLine($"[UnshareMemoAsync] Unshared successfully");
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw new InvalidOperationException($"メモの共有解除に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// APIのDTOからアプリのModelに変換
    /// </summary>
    private Memo ConvertFromApiDto(ApiMemoDto apiDto)
    {
        return new Memo
        {
            Id = apiDto.Id,
            Title = apiDto.Title,
            Content = apiDto.Content,
            AuthorId = apiDto.AuthorId,
            IsShared = apiDto.IsShared,
            CreatedAt = apiDto.CreatedAt,
            UpdatedAt = apiDto.UpdatedAt
        };
    }
}
