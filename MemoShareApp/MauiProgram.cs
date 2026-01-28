using Microsoft.Extensions.Logging;
using MemoShareApp.Services;
using MemoShareApp.ViewModels;
using MemoShareApp.Views;
using MemoShareApp.Converters;
using MemoShareApp.Data;
using SQLitePCL;

namespace MemoShareApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Initialize SQLite for all platforms
		Batteries_V2.Init();
		
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register Database
		string dbPath = Path.Combine(FileSystem.AppDataDirectory, "memoshare.db3");
		
		// デバッグ: データベースパスをログ出力
		System.Diagnostics.Debug.WriteLine($"=== Database Path ===");
		System.Diagnostics.Debug.WriteLine(dbPath);
		System.Diagnostics.Debug.WriteLine($"====================");
		
		builder.Services.AddSingleton(s => new AppDatabase(dbPath));

		// Register HttpClient
		builder.Services.AddHttpClient<IMemoService, ApiMemoService>(client =>
		{
			// TODO: appsettings.jsonまたは環境変数から取得
			client.BaseAddress = new Uri("http://your-vps-domain.com:5000/api/");
			client.Timeout = TimeSpan.FromSeconds(30);
		});

		// Register Services
		builder.Services.AddSingleton<IAuthService, AuthService>();
		// Note: ApiMemoServiceはHttpClientで登録済み
		// ローカルのみ使用する場合は以下をコメント解除
		// builder.Services.AddSingleton<IMemoService, MemoService>();

		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<MemoListViewModel>();
		builder.Services.AddTransient<SharedMemoListViewModel>();
		builder.Services.AddTransient<MemoDetailViewModel>();

		// Register Pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<MemoListPage>();
		builder.Services.AddTransient<SharedMemoListPage>();
		builder.Services.AddTransient<MemoDetailPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

