using System.ComponentModel.DataAnnotations;

namespace MemoShareApp.API.Models;

/// <summary>
/// ユーザーモデル
/// </summary>
public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // ナビゲーションプロパティ
    public virtual ICollection<Memo> Memos { get; set; } = new List<Memo>();
    public virtual ICollection<MemoShare> SharedMemos { get; set; } = new List<MemoShare>();
}
