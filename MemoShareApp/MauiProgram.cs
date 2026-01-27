using Microsoft.Extensions.Logging;
using MemoShareApp.Services;
using MemoShareApp.ViewModels;
using MemoShareApp.Views;
using MemoShareApp.Converters;

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

		// Register Services
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<IMemoService, MemoService>();

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
