using SQLite;

namespace MemoShareApp.Models;

/// <summary>
/// メモの共有関係を表すモデル
/// </summary>
public class MemoShare
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [Indexed]
    public string MemoId { get; set; } = string.Empty;
    
    [Indexed]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime SharedAt { get; set; } = DateTime.Now;
}
