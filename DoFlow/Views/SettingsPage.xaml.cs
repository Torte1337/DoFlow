using DoFlow.ViewModels;

namespace DoFlow.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsPageModel pm)
	{
		InitializeComponent();
		BindingContext = pm;
	}
}