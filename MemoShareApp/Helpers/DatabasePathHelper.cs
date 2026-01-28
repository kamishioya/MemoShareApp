namespace MemoShareApp.Helpers;

public static class DatabasePathHelper
{
    /// <summary>
    /// データベースファイルのフルパスを取得します。
    /// デバッグ時にログ出力して確認できます。
    /// </summary>
    public static string GetDatabasePath()
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "memoshare.db3");
        
        // デバッグ出力
        System.Diagnostics.Debug.WriteLine($"[Database Path] {dbPath}");
        Console.WriteLine($"[Database Path] {dbPath}");
        
        return dbPath;
    }
    
    /// <summary>
    /// データベースファイルが存在するか確認します。
    /// </summary>
    public static bool DatabaseExists()
    {
        string dbPath = GetDatabasePath();
        bool exists = File.Exists(dbPath);
        
        System.Diagnostics.Debug.WriteLine($"[Database Exists] {exists}");
        Console.WriteLine($"[Database Exists] {exists}");
        
        return exists;
    }
}
