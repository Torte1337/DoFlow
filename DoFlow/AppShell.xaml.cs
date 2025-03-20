using DoFlow.Views;

namespace DoFlow;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(AuthPage),typeof(AuthPage));
		Routing.RegisterRoute(nameof(SettingsPage),typeof(SettingsPage));
		Routing.RegisterRoute(nameof(DashboardPage),typeof(DashboardPage));
	}
}
