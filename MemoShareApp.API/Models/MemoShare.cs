using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemoShareApp.API.Models;

/// <summary>
/// メモ共有モデル
/// </summary>
public class MemoShare
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string MemoId { get; set; } = string.Empty;
    
    [Required]
    public string SharedWithUserId { get; set; } = string.Empty;
    
    public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    
    // ナビゲーションプロパティ
    [ForeignKey(nameof(MemoId))]
    public virtual Memo? Memo { get; set; }
    
    [ForeignKey(nameof(SharedWithUserId))]
    public virtual User? SharedWithUser { get; set; }
}
