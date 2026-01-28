using SQLite;

namespace MemoShareApp.Models;

public class User
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Unique]
    public string Username { get; set; } = string.Empty;
    
    [Unique]
    public string Email { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

