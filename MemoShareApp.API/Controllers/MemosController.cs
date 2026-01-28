using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemoShareApp.API.Data;
using MemoShareApp.API.DTOs;
using MemoShareApp.API.Models;
using System.Security.Claims;

namespace MemoShareApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MemosController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<MemosController> _logger;

    public MemosController(ApiDbContext context, ILogger<MemosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst("userId")?.Value 
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("ユーザーIDが見つかりません");
    }

    /// <summary>
    /// 自分のメモ一覧を取得
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<MemoDto>>> GetMyMemos()
    {
        try
        {
            var userId = GetCurrentUserId();
            var memos = await _context.Memos
                .Include(m => m.Author)
                .Where(m => m.AuthorId == userId)
                .OrderByDescending(m => m.UpdatedAt)
                .Select(m => new MemoDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Content = m.Content,
                    AuthorId = m.AuthorId,
                    AuthorName = m.Author!.DisplayName,
                    IsShared = m.IsShared,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return Ok(memos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user memos");
            return StatusCode(500, new { message = "メモの取得中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// 共有されているメモ一覧を取得
    /// </summary>
    [HttpGet("shared")]
    public async Task<ActionResult<List<MemoDto>>> GetSharedMemos()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // 自分に共有されたメモ
            var sharedWithMeMemos = await _context.MemoShares
                .Include(ms => ms.Memo)
                .ThenInclude(m => m!.Author)
                .Where(ms => ms.SharedWithUserId == userId)
                .Select(ms => new MemoDto
                {
                    Id = ms.Memo!.Id,
                    Title = ms.Memo.Title,
                    Content = ms.Memo.Content,
                    AuthorId = ms.Memo.AuthorId,
                    AuthorName = ms.Memo.Author!.DisplayName,
                    IsShared = ms.Memo.IsShared,
                    CreatedAt = ms.Memo.CreatedAt,
                    UpdatedAt = ms.Memo.UpdatedAt
                })
                .ToListAsync();

            return Ok(sharedWithMeMemos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shared memos");
            return StatusCode(500, new { message = "共有メモの取得中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// 特定のメモを取得
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MemoDto>> GetMemo(string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var memo = await _context.Memos
                .Include(m => m.Author)
                .Include(m => m.Shares)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (memo == null)
            {
                return NotFound(new { message = "メモが見つかりません" });
            }

            // アクセス権限チェック（作成者または共有されているユーザー）
            var hasAccess = memo.AuthorId == userId || 
                           memo.Shares.Any(s => s.SharedWithUserId == userId);

            if (!hasAccess)
            {
                return Forbid();
            }

            var memoDto = new MemoDto
            {
                Id = memo.Id,
                Title = memo.Title,
                Content = memo.Content,
                AuthorId = memo.AuthorId,
                AuthorName = memo.Author!.DisplayName,
                IsShared = memo.IsShared,
                CreatedAt = memo.CreatedAt,
                UpdatedAt = memo.UpdatedAt
            };

            return Ok(memoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memo");
            return StatusCode(500, new { message = "メモの取得中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// メモを作成
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MemoDto>> CreateMemo(CreateMemoRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var memo = new Memo
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                Content = request.Content,
                AuthorId = userId,
                IsShared = request.IsShared,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Memos.Add(memo);
            await _context.SaveChangesAsync();

            // 作成されたメモを取得（Authorを含む）
            var createdMemo = await _context.Memos
                .Include(m => m.Author)
                .FirstAsync(m => m.Id == memo.Id);

            var memoDto = new MemoDto
            {
                Id = createdMemo.Id,
                Title = createdMemo.Title,
                Content = createdMemo.Content,
                AuthorId = createdMemo.AuthorId,
                AuthorName = createdMemo.Author!.DisplayName,
                IsShared = createdMemo.IsShared,
                CreatedAt = createdMemo.CreatedAt,
                UpdatedAt = createdMemo.UpdatedAt
            };

            return CreatedAtAction(nameof(GetMemo), new { id = memo.Id }, memoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating memo");
            return StatusCode(500, new { message = "メモの作成中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// メモを更新
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MemoDto>> UpdateMemo(string id, UpdateMemoRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var memo = await _context.Memos
                .Include(m => m.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (memo == null)
            {
                return NotFound(new { message = "メモが見つかりません" });
            }

            // 作成者のみ更新可能
            if (memo.AuthorId != userId)
            {
                return Forbid();
            }

            memo.Title = request.Title;
            memo.Content = request.Content;
            memo.IsShared = request.IsShared;
            memo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var memoDto = new MemoDto
            {
                Id = memo.Id,
                Title = memo.Title,
                Content = memo.Content,
                AuthorId = memo.AuthorId,
                AuthorName = memo.Author!.DisplayName,
                IsShared = memo.IsShared,
                CreatedAt = memo.CreatedAt,
                UpdatedAt = memo.UpdatedAt
            };

            return Ok(memoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating memo");
            return StatusCode(500, new { message = "メモの更新中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// メモを削除
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMemo(string id)
    {
        try
        {
            var userId = GetCurrentUserId();

            var memo = await _context.Memos.FirstOrDefaultAsync(m => m.Id == id);

            if (memo == null)
            {
                return NotFound(new { message = "メモが見つかりません" });
            }

            // 作成者のみ削除可能
            if (memo.AuthorId != userId)
            {
                return Forbid();
            }

            _context.Memos.Remove(memo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting memo");
            return StatusCode(500, new { message = "メモの削除中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// メモを共有
    /// </summary>
    [HttpPost("{id}/share")]
    public async Task<IActionResult> ShareMemo(string id, ShareMemoRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var memo = await _context.Memos.FirstOrDefaultAsync(m => m.Id == id);

            if (memo == null)
            {
                return NotFound(new { message = "メモが見つかりません" });
            }

            // 作成者のみ共有可能
            if (memo.AuthorId != userId)
            {
                return Forbid();
            }

            // 共有先ユーザーが存在するか確認
            var targetUser = await _context.Users.FindAsync(request.SharedWithUserId);
            if (targetUser == null)
            {
                return BadRequest(new { message = "共有先のユーザーが見つかりません" });
            }

            // 既に共有されているかチェック
            var existingShare = await _context.MemoShares
                .FirstOrDefaultAsync(ms => ms.MemoId == id && ms.SharedWithUserId == request.SharedWithUserId);

            if (existingShare != null)
            {
                return BadRequest(new { message = "このユーザーには既に共有されています" });
            }

            var memoShare = new MemoShare
            {
                Id = Guid.NewGuid().ToString(),
                MemoId = id,
                SharedWithUserId = request.SharedWithUserId,
                SharedAt = DateTime.UtcNow
            };

            _context.MemoShares.Add(memoShare);
            await _context.SaveChangesAsync();

            return Ok(new { message = "メモを共有しました" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing memo");
            return StatusCode(500, new { message = "メモの共有中にエラーが発生しました" });
        }
    }

    /// <summary>
    /// メモの共有を解除
    /// </summary>
    [HttpDelete("{id}/share/{userId}")]
    public async Task<IActionResult> UnshareMemo(string id, string userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var memo = await _context.Memos.FirstOrDefaultAsync(m => m.Id == id);

            if (memo == null)
            {
                return NotFound(new { message = "メモが見つかりません" });
            }

            // 作成者のみ共有解除可能
            if (memo.AuthorId != currentUserId)
            {
                return Forbid();
            }

            var memoShare = await _context.MemoShares
                .FirstOrDefaultAsync(ms => ms.MemoId == id && ms.SharedWithUserId == userId);

            if (memoShare == null)
            {
                return NotFound(new { message = "共有情報が見つかりません" });
            }

            _context.MemoShares.Remove(memoShare);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsharing memo");
            return StatusCode(500, new { message = "共有解除中にエラーが発生しました" });
        }
    }
}
