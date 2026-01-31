using MemoShareApp.Views;

namespace MemoShareApp;

public partial class AppShell : Shell
{
	public AppShell(IServiceProvider serviceProvider)
	{
		InitializeComponent();
		
		// Register routes for navigation with factory
		Routing.RegisterRoute(nameof(MemoDetailPage), typeof(MemoDetailPage));
		Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
		
		// Set up page resolver for DI
		SetItemTemplate(serviceProvider);
	}
	
	private void SetItemTemplate(IServiceProvider serviceProvider)
	{
		// Find ShellContents and set their content factories
		foreach (var item in Items)
		{
			if (item is TabBar tabBar)
			{
				foreach (var section in tabBar.Items)
				{
					foreach (var content in section.Items)
					{
						if (content.Route == "MemoListPage")
						{
							content.ContentTemplate = new DataTemplate(() => 
								serviceProvider.GetRequiredService<MemoListPage>());
						}
						else if (content.Route == "SharedMemoListPage")
						{
							content.ContentTemplate = new DataTemplate(() => 
								serviceProvider.GetRequiredService<SharedMemoListPage>());
						}
					}
				}
			}
		}
	}
}
