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
}