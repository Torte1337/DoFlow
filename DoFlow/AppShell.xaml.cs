using DoFlow.Views;

namespace DoFlow;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(AuthPage),typeof(AuthPage));
		Routing.RegisterRoute(nameof(PersonalTaskPage),typeof(PersonalTaskPage));
		Routing.RegisterRoute(nameof(TeamTaskPage),typeof(TeamTaskPage));
		Routing.RegisterRoute(nameof(SettingsPage),typeof(SettingsPage));
	}
}
