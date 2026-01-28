using SQLite;

namespace MemoShareApp.Models;

public class Memo
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    [Indexed]
    public string AuthorId { get; set; } = string.Empty;
    
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public bool IsShared { get; set; } = false;
    
    [Ignore]
    public List<string> SharedWithUserIds { get; set; } = new();
}

