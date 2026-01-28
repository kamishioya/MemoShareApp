?using MemoShareApp.Models;
using MemoShareApp.Models.Api;
using MemoShareApp.Data;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace MemoShareApp.Services;

/// <summary>
/// API経由でメモを管理するサービス（オフライン同期対応）
/// </summary>
public class ApiMemoService : IMemoService
{
    private readonly HttpClient _httpClient;
    private readonly AppDatabase _localDatabase;
    private readonly IAuthService _authService;
    private readonly string _apiBaseUrl;

    public ApiMemoService(HttpClient httpClient, AppDatabase localDatabase, IAuthService authService)
    {
        _httpClient = httpClient;
        _localDatabase = localDatabase;
        _authService = authService;
        
        // TODO: appsettings.jsonから取得するように変更
        _apiBaseUrl = "http://your-vps-domain.com:5000/api";
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
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

    private async Task<bool> IsOnlineAsync()
    {
        try
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
                return false;

            // サーバーへのpingテスト
            var response = await _httpClient.GetAsync("/health", 
                new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 現在のユーザーのメモ一覧を取得（オンライン優先、オフライン時はローカル）
    /// </summary>
    public async Task<List<Memo>> GetMyMemosAsync()
    {
        var isOnline = await IsOnlineAsync();

        if (isOnline)
        {
            try
            {
                SetAuthToken();
                var apiMemos = await _httpClient.GetFromJsonAsync<List<ApiMemoDto>>("memos/my");
                
                if (apiMemos != null)
                {
                    // APIから取得したデータをローカルに同期
                    var memos = apiMemos.Select(ConvertFromApiDto).ToList();
                    await SyncLocalMemosAsync(memos);
                    return memos;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
                // オンラインだがAPIエラーの場合はローカルにフォールバック
            }
        }

        // オフライン時またはAPI失敗時はローカルから取得
        return await _localDatabase.GetMemosByAuthorAsync(_authService.CurrentUser?.Id ?? "");
    }

    /// <summary>
    /// 共有されているメモ一覧を取得
    /// </summary>
    public async Task<List<Memo>> GetSharedMemosAsync()
    {
        var isOnline = await IsOnlineAsync();

        if (isOnline)
        {
            try
            {
                SetAuthToken();
                var apiMemos = await _httpClient.GetFromJsonAsync<List<ApiMemoDto>>("memos/shared");
                
                if (apiMemos != null)
                {
                    return apiMemos.Select(ConvertFromApiDto).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            }
        }

        // 共有メモはオンラインのみ対応（ローカルでは複雑なため）
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return new List<Memo>();

        return await _localDatabase.GetSharedMemosAsync(currentUser.Id);
    }

    /// <summary>
    /// 指定されたIDのメモを取得
    /// </summary>
    public async Task<Memo?> GetMemoByIdAsync(string id)
    {
        var isOnline = await IsOnlineAsync();

        if (isOnline)
        {
            try
            {
                SetAuthToken();
                var apiMemo = await _httpClient.GetFromJsonAsync<ApiMemoDto>($"memos/{id}");
                
                if (apiMemo != null)
                {
                    return ConvertFromApiDto(apiMemo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            }
        }

        return await _localDatabase.GetMemoByIdAsync(id);
    }

    /// <summary>
    /// 新しいメモを作成
    /// </summary>
    public async Task<Memo> CreateMemoAsync(Memo memo)
    {
        var isOnline = await IsOnlineAsync();

        if (isOnline)
        {
            try
            {
                SetAuthToken();
                var request = new CreateMemoRequest
                {
                    Title = memo.Title,
                    Content = memo.Content,
                    IsShared = memo.IsShared
                };

                var response = await _httpClient.PostAsJsonAsync("memos", request);
                response.EnsureSuccessStatusCode();

                var apiMemo = await response.Content.ReadFromJsonAsync<ApiMemoDto>();
                if (apiMemo != null)
                {
                    var createdMemo = ConvertFromApiDto(apiMemo);
                    await _localDatabase.SaveMemoAsync(createdMemo);
                    return createdMemo;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
                // オンラインだがエラーの場合は一旦ローカル保存（後で同期）
            }
        }

        // オフライン時はローカルに保存
        memo.Id = Guid.NewGuid().ToString();
        memo.CreatedAt = DateTime.UtcNow;
        memo.UpdatedAt = DateTime.UtcNow;
        await _localDatabase.SaveMemoAsync(memo);
        return memo;
    }

    /// <summary>
    /// 既存のメモを更新
    /// </summary>
    public async Task<Memo> UpdateMemoAsync(Memo memo)
    {
        var isOnline = await IsOnlineAsync();

        if (isOnline)
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

                var response = await _httpClient.PutAsJsonAsync($"memos/{memo.Id}", request);
                response.EnsureSuccessStatusCode();

                var apiMemo = await response.Content.ReadFromJsonAsync<ApiMemoDto>();
                if (apiMemo != null)
                {
                    var updatedMemo = ConvertFromApiDto(apiMemo);
                    await _localDatabase.SaveMemoAsync(updatedMemo);
                    return updatedMemo;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            }
        }

        // オフライン時はローカル更新
        memo.UpdatedAt = DateTime.UtcNow;
        await _localDatabase.SaveMemoAsync(memo);
        return memo;
    }

    /// <summary>
    /// 指定されたIDのメモを削除
    /// </summary>
    public async Task DeleteMemoAsync(string id)
    {
        var isOnline = await IsOnlineAsync();

        if (isOnline)
        {
            try
            {
                SetAuthToken();
                var response = await _httpClient.DeleteAsync($"memos/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            }
        }

        // ローカルからも削除
        await _localDatabase.DeleteMemoAsync(id);
    }

    /// <summary>
    /// メモを指定されたユーザーと共有
    /// </summary>
    public async Task ShareMemoAsync(string memoId, string userId)
    {
        var isOnline = await IsOnlineAsync();

        if (!isOnline)
        {
            throw new InvalidOperationException("共有機能はオンライン時のみ利用可能です");
        }

        try
        {
            SetAuthToken();
            var request = new ShareMemoRequest
            {
                SharedWithUserId = userId
            };

            var response = await _httpClient.PostAsJsonAsync($"memos/{memoId}/share", request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// メモの共有を解除
    /// </summary>
    public async Task UnshareMemoAsync(string memoId, string userId)
    {
        var isOnline = await IsOnlineAsync();

        if (!isOnline)
        {
            throw new InvalidOperationException("共有解除はオンライン時のみ利用可能です");
        }

        try
        {
            SetAuthToken();
            var response = await _httpClient.DeleteAsync($"memos/{memoId}/share/{userId}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error] {ex.Message}");
            throw;
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

    /// <summary>
    /// ローカルデータベースを同期
    /// </summary>
    private async Task SyncLocalMemosAsync(List<Memo> apiMemos)
    {
        try
        {
            foreach (var memo in apiMemos)
            {
                await _localDatabase.SaveMemoAsync(memo);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Sync Error] {ex.Message}");
        }
    }
}
