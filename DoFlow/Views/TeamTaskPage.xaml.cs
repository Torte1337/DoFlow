using DoFlow.Models;
using DoFlow.ViewModels;

namespace DoFlow.Views;

public partial class TeamTaskPage : ContentPage
{
	public TeamTaskPage(TeamTaskPageModel pm)
	{
		InitializeComponent();
		BindingContext = pm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
		var pm = (TeamTaskPageModel)BindingContext;
		// pm.OnRefreshTeamList();
		// pm.OnRefreshToDoList();
    }
    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		if(sender is CheckBox checkBox)
		{
			var todoModel = (TodoModel)checkBox.BindingContext;

			if(BindingContext is TeamTaskPageModel viewModel)
				OnSendToViewModel(viewModel,todoModel);
		}
    }
	private async void OnSendToViewModel(TeamTaskPageModel viewModel,TodoModel model)
	{
		await viewModel.OnCheckIsChanged(model);
	}
}