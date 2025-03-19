using DoFlow.ViewModels;

namespace DoFlow.Views;

public partial class AuthPage : ContentPage
{
	public AuthPage(AuthPageModel pm)
	{
		InitializeComponent();
		BindingContext = pm;
	}
}