using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemoShareApp.API.Data;
using MemoShareApp.API.DTOs;
using MemoShareApp.API.Models;
using MemoShareApp.API.Services;
using BCrypt.Net;

namespace MemoShareApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApiDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// ユーザー登録
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        try
        {
            // ユーザー名の重複チェック
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "ユーザー名は既に使用されています" });
            }

            // メールアドレスの重複チェック
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "メールアドレスは既に使用されています" });
            }

            // パスワードをハッシュ化
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 新しいユーザーを作成
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                DisplayName = request.DisplayName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // JWTトークンを生成
            var token = _tokenService.GenerateToken(user.Id, user.Username);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new { message = "ユーザー登録中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// ログイン
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            // ユーザーを検索
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "ユーザー名またはパスワードが正しくありません" });
            }

            // パスワードを検証
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "ユーザー名またはパスワードが正しくありません" });
            }

            // JWTトークンを生成
            var token = _tokenService.GenerateToken(user.Id, user.Username);

            return Ok(new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "ログイン中にエラーが発生しました" });
        }
    }
}
