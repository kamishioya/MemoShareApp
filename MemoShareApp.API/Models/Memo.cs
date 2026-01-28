using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemoShareApp.API.Models;

/// <summary>
/// メモモデル
/// </summary>
public class Memo
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string AuthorId { get; set; } = string.Empty;
    
    public bool IsShared { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // ナビゲーションプロパティ
    [ForeignKey(nameof(AuthorId))]
    public virtual User? Author { get; set; }
    
    public virtual ICollection<MemoShare> Shares { get; set; } = new List<MemoShare>();
}
