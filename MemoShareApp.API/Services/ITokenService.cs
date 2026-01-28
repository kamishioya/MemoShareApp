namespace MemoShareApp.API.Services;

/// <summary>
/// JWTトークン生成サービス
/// </summary>
public interface ITokenService
{
    string GenerateToken(string userId, string username);
}
