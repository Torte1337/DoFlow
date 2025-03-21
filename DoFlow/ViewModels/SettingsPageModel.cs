using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoFlow.Manager;
using DoFlow.Models;
using DoFlow.ViewModels.ViewModelBase;
using DoFlow.Views;
using CommunityToolkit.Mvvm.Messaging;
using DoFlow.Messages;
using Android.OS;

namespace DoFlow.ViewModels;

public partial class SettingsPageModel : BaseViewModel
{
    private readonly DatabaseManager manager;


    [ObservableProperty] private ObservableCollection<TeamModel> _teams = new ObservableCollection<TeamModel>();
    [ObservableProperty] private string teamidField;

    public SettingsPageModel(DatabaseManager mgr)
    {
        manager = mgr;
        OnLoadData();
        WeakReferenceMessenger.Default.Register<MessageUserDeleted>(this,OnCleanMethod);
    }
    private void OnCleanMethod(object recipient, MessageUserDeleted msg)
    {
        Teams.Clear();
        TeamidField = "";
    }
    private async void OnLoadData()
    {
        if(Teams is {Count: > 0})
            Teams.Clear();

        Teams = new ObservableCollection<TeamModel>(await manager.OnGetUserTeams());
    }
    [RelayCommand]
    private async Task OnCreateTeamButton()
    {
        var result1 = await Shell.Current.DisplayPromptAsync("Team anlegen", "Bitte gebe eine ID ein","Ok","Abbrechen","123456",8,Keyboard.Numeric);
        if(!string.IsNullOrEmpty(result1))
        {
            var teamname = await Shell.Current.DisplayPromptAsync("Team anlegen","Bitte gebe ein Teamname ein","Ok","Abbrechen","Mein Teamname...");
            if(!string.IsNullOrEmpty(teamname))
            {
                TeamModel newTeam = new TeamModel
                {
                    Id = Guid.NewGuid().ToString(),
                    TeamId = result1,
                    Name = teamname,
                    AdminId = manager.ActiveUser.Id,
                    MemberIds = new List<string>
                    {
                        manager.ActiveUser.Id
                    },
                    Tasks = new List<TodoModel>()
                };
                if(await manager.OnCreateNewTeam(newTeam))
                {
                    Teams.Add(newTeam);
                }
            }
        }
    }

    [RelayCommand]
    private async Task OnLeaveTeam(string teamid)
    {
        if(await manager.OnLeaveTeam(teamid))
        {
            await Shell.Current.DisplayAlert("Info", "Das wurde verlassen","Ok");
            OnLoadData();
        }
    }
    [RelayCommand]
    private async Task OnJoinTeam()
    {
        if(await manager.OnJoinTeam(TeamidField))
        {
            TeamidField = "";
            await Shell.Current.DisplayAlert("Info","Sie sind nun im Team","Ok");
        }
        else
        {
            await Shell.Current.DisplayAlert("Fehler","Beim beitreten des Teams ist ein Fehler aufgetreten.","Ok");
            TeamidField = "";
            return;
        }
    }
    [RelayCommand]
    private async Task OnDeleteMyAccount()
    {
        if(await manager.OnDeleteAccount())
        {
            await Shell.Current.DisplayAlert("Info","Dein Account wurde gelöscht, du wirst automatisch zur Anmeldeseite weitergeleitet","Ok");
            await Shell.Current.GoToAsync($"//{nameof(AuthPage)}");
        }
    }


}
