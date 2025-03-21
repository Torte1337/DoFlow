using DoFlow.Models;
using DoFlow.ViewModels;

namespace DoFlow.Views;

public partial class PersonalTaskPage : ContentPage
{
	public PersonalTaskPage(PersonalTaskPageModel pm)
	{
		InitializeComponent();
		BindingContext = pm;
	}

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		if(sender is CheckBox checkBox)
		{
			var todoModel = (TodoModel)checkBox.BindingContext;

			if(BindingContext is PersonalTaskPageModel viewModel)
				OnSendToViewModel(viewModel,todoModel);
		}
    }
	private async void OnSendToViewModel(PersonalTaskPageModel viewModel,TodoModel model)
	{
		await viewModel.OnCheckIsChanged(model);
	}
}