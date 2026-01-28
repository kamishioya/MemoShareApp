namespace MemoShareApp.API.DTOs;

/// <summary>
/// メモDTO
/// </summary>
public class MemoDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// メモ作成リクエスト
/// </summary>
public class CreateMemoRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsShared { get; set; } = false;
}

/// <summary>
/// メモ更新リクエスト
/// </summary>
public class UpdateMemoRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsShared { get; set; } = false;
}

/// <summary>
/// メモ共有リクエスト
/// </summary>
public class ShareMemoRequest
{
    public string SharedWithUserId { get; set; } = string.Empty;
}
