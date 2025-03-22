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
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		var viewModel = BindingContext as TeamTaskPageModel;
		if (viewModel != null)
		{
		    await viewModel.LoadTeams();
		}
	}
}