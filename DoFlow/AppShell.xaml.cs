using DoFlow.Views;

namespace DoFlow;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(AuthPage),typeof(AuthPage));
		Routing.RegisterRoute(nameof(DashboardPage),typeof(DashboardPage));
		Routing.RegisterRoute(nameof(SettingsPage),typeof(SettingsPage));
	}
    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {

    }
}
