using DoFlow.Views;

namespace DoFlow;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(AuthPage),typeof(AuthPage));
	}
}
