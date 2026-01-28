using Microsoft.EntityFrameworkCore;
using MemoShareApp.API.Models;

namespace MemoShareApp.API.Data;

/// <summary>
/// API用データベースコンテキスト
/// </summary>
public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Memo> Memos { get; set; }
    public DbSet<MemoShare> MemoShares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Userエンティティの設定
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Memoエンティティの設定
        modelBuilder.Entity<Memo>(entity =>
        {
            entity.HasOne(m => m.Author)
                  .WithMany(u => u.Memos)
                  .HasForeignKey(m => m.AuthorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // MemoShareエンティティの設定
        modelBuilder.Entity<MemoShare>(entity =>
        {
            entity.HasOne(ms => ms.Memo)
                  .WithMany(m => m.Shares)
                  .HasForeignKey(ms => ms.MemoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ms => ms.SharedWithUser)
                  .WithMany(u => u.SharedMemos)
                  .HasForeignKey(ms => ms.SharedWithUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // 同じメモを同じユーザーに複数回共有しないように
            entity.HasIndex(ms => new { ms.MemoId, ms.SharedWithUserId }).IsUnique();
        });
    }
}
