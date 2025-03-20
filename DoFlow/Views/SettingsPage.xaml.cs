using DoFlow.ViewModels;

namespace DoFlow.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsPageModel pm)
	{
		InitializeComponent();
		BindingContext = pm;
	}
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
		var pm = (SettingsPageModel)BindingContext;
		if(pm != null)
			pm.OnStopListening();
    }
}