using Microsoft.Extensions.Logging;
using MemoShareApp.Services;
using MemoShareApp.ViewModels;
using MemoShareApp.Views;
using MemoShareApp.Converters;
using MemoShareApp.Data;
using MemoShareApp.Helpers;

namespace MemoShareApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register HttpClient for API
		builder.Services.AddHttpClient<IMemoService, ApiMemoService>(client =>
		{
			// ローカルDocker環境用
			// Windows: localhost, Androidエミュレータ: 10.0.2.2, 実機: PCのIPアドレス
#if ANDROID
			client.BaseAddress = new Uri("http://10.0.2.2:5000/api/");
#else
			client.BaseAddress = new Uri("http://localhost:5000/api/");
#endif
			client.Timeout = TimeSpan.FromSeconds(10);
		});

		// Register Database
		builder.Services.AddSingleton<AppDatabase>(sp => 
			new AppDatabase(DatabasePathHelper.GetDatabasePath()));

		// Register Services
		builder.Services.AddSingleton<IAuthService, AuthService>();

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
		builder.Services.AddTransient<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

