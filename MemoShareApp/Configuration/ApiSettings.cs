?namespace MemoShareApp.Configuration;

/// <summary>
/// API設定
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// API ベースURL
    /// 例: https://your-domain.com/api または http://your-vps-ip:5000/api
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5000/api";
    
    /// <summary>
    /// タイムアウト（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// オフラインモード有効
    /// </summary>
    public bool EnableOfflineMode { get; set; } = true;
}
