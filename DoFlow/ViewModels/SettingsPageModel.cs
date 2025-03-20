using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoFlow.Manager;
using DoFlow.Models;
using DoFlow.ViewModels.ViewModelBase;

namespace DoFlow.ViewModels;

public partial class SettingsPageModel : BaseViewModel
{
    private readonly DatabaseManager manager;


    [ObservableProperty] private ObservableCollection<TeamModel> _teams = new ObservableCollection<TeamModel>();
    [ObservableProperty] private string teamidField;
    public SettingsPageModel(DatabaseManager mgr)
    {
        manager = mgr;
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
                    AdminId = manager.ActiveUser.Id
                };
                await manager.OnAddTeamToDatabase(newTeam,manager.ActiveUser);
            }
        }
    }

    [RelayCommand]
    private async Task OnDeleteTeam()
    {

    }
    [RelayCommand]
    private async Task OnLeaveTeam(string teamid)
    {
        if(await manager.OnLeaveTeam(teamid,manager.ActiveUser.Id))
        {
            await Shell.Current.DisplayAlert("Info", "Das wurde verlassen","Ok");
        }
    }
    [RelayCommand]
    private async Task OnJoinTeam()
    {
        if(await manager.OnSearchTeam(TeamidField))
        {
            if(await manager.OnJoinTeam(TeamidField,manager.ActiveUser))
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
        else
        {
            await Shell.Current.DisplayAlert("Fehler","Das Team mit dieser ID: " +  TeamidField  + " wurde nicht gefunden","Ok");
            TeamidField = "";
            return;
        }
    }
}
