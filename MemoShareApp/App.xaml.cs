using MemoShareApp.Views;

namespace MemoShareApp;

public partial class App : Application
{
	public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();
		_serviceProvider = serviceProvider;
	}

	private readonly IServiceProvider _serviceProvider;

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Start with login page
		var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
		return new Window(new NavigationPage(loginPage));
	}
}