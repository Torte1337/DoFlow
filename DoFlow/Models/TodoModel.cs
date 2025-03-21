using CommunityToolkit.Mvvm.ComponentModel;

namespace DoFlow.Models;

public partial class TodoModel : ObservableObject
{
    public string Id {get;set;}
    public string Title {get;set;}
    public string OwnerId {get;set;}
    public string? TeamId {get;set;}

    [ObservableProperty]
    private bool isChecked;
}
