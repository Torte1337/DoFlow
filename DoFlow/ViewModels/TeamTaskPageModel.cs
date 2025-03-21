using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoFlow.Manager;
using DoFlow.Models;
using DoFlow.Services;
using DoFlow.ViewModels.ViewModelBase;

namespace DoFlow.ViewModels;

public partial class TeamTaskPageModel : BaseViewModel
{
    private readonly DatabaseManager manager;
    private readonly FirebaseService _firebaseService;
    [ObservableProperty] private ObservableCollection<TeamModel> _teams = new ObservableCollection<TeamModel>();
    [ObservableProperty] private ObservableCollection<TodoModel> _teamToDos = new ObservableCollection<TodoModel>();
    [ObservableProperty] private string selectedTeam = "";


    public TeamTaskPageModel(DatabaseManager mgr, FirebaseService service)
    {
        manager = mgr;
        _firebaseService = service;

        _firebaseService.OnSubscribeToTeamChanges(team =>
        {
            if(!Teams.Any(t => t.Id == team.Id))
            {
                MainThread.BeginInvokeOnMainThread(() => 
                {
                    Teams.Add(team);
                });
            }
        });
    }
    
    public async void OnRefreshTeamList()
    {
        if(Teams is {Count: > 0})
            Teams.Clear();

        var list = await manager.OnGetUserTeams();

        foreach(var item in list)
            Teams.Add(item);

    }
    public async void OnRefreshToDoList()
    {
        if(TeamToDos is {Count: > 0})
            TeamToDos.Clear();
        
        if(string.IsNullOrEmpty(SelectedTeam))
            return;

        var list = await manager.OnGetTeamTasks(SelectedTeam);
        TeamToDos = new ObservableCollection<TodoModel>(list);
    }

    public async Task OnCheckIsChanged(TodoModel model)
    {
        // if(await manager.OnUpdateTask(manager.ActiveUser.Id,model))
        //     OnRefreshToDoList();
    }

    [RelayCommand]
    private async Task OnAddTaskToTeam()
    {
        if(string.IsNullOrEmpty(SelectedTeam))
        {
            await Shell.Current.DisplayAlert("Fehler", "Es muss ein Team ausgewählt sein.","Ok");
            return;
        }
        var action = await Shell.Current.DisplayPromptAsync("Aufgabenbenennung", "Bitte gebe deiner Aufgabe einen Namen...","Speichern","Abbrechen","Dein Aufgabenname...");
        if(!string.IsNullOrEmpty(action))
        {
            TodoModel newTeamTask = new TodoModel
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = manager.ActiveUser.Id,
                TeamId = SelectedTeam,
                Title = action,
                IsChecked = false
            };

            if(await manager.OnAddTodo(newTeamTask))
                OnRefreshToDoList();
        }
    }

}
