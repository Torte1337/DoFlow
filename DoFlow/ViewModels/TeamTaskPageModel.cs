using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoFlow.Manager;
using DoFlow.Models;
using DoFlow.Services;
using System.Linq;
using System.Threading.Tasks;
using DoFlow.ViewModels.ViewModelBase;

namespace DoFlow.ViewModels
{
    public partial class TeamTaskPageModel : BaseViewModel
    {
        private readonly DatabaseManager manager;
        private readonly FirebaseService _firebaseService;

        [ObservableProperty]
        private ObservableCollection<TeamModel> teams = new ObservableCollection<TeamModel>();

        [ObservableProperty]
        private ObservableCollection<TodoModel> teamToDos = new ObservableCollection<TodoModel>();

        [ObservableProperty]
        private TeamModel selectedTeam;

        public TeamTaskPageModel(DatabaseManager mgr, FirebaseService service)
        {
            manager = mgr;
            _firebaseService = service;
        }

        // Methode zum Laden der Teams
        public async Task LoadTeams()
        {
            try
            {
                if (Teams.Count > 0)
                    Teams.Clear();

                var list = await manager.OnGetUserTeams();

                if(list == null)
                    return;
                    
                foreach (var item in list)
                    Teams.Add(item);

                _firebaseService.OnSubscribeToTeamChanges(team =>
                {
                    if (!Teams.Any(t => t.Id == team.Id))
                    {
                        Teams.Add(team);
                    }
                });

                if(Teams is {Count: > 0})
                    SelectedTeam = Teams.First();
            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Fehler",ex.Message,"Ok");
                return;
            }
            
        }

        // Methode zum Laden der Aufgaben eines ausgewählten Teams
        public async Task LoadTasksForSelectedTeam()
        {
            try
            {
                if (TeamToDos.Count > 0)
                    TeamToDos.Clear();

                if (SelectedTeam == null)
                    return;

                var list = await manager.OnGetTeamTasks(SelectedTeam.TeamId);

                if(list == null)
                {
                    await Shell.Current.DisplayAlert("Fehler","Beim laden ist ein Fehler aufgetreten", "Ok");
                    return;
                }
                var sortedList = list.OrderBy(x => x.IsChecked).ToList();
                foreach (var todo in sortedList)
                {
                    todo.PropertyChanged += ToDo_PropertyChanged;
                    TeamToDos.Add(todo);
                }
            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Fehler",ex.Message,"Ok");
                return;
            }
        }

        // PropertyChanged-Handler für Aufgaben, wenn "IsChecked" geändert wird
        private async void ToDo_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is TodoModel todo && e.PropertyName == nameof(TodoModel.IsChecked))
            {
                await OnCheckIsChanged(todo);
            }
        }

        // Methode, um den Status einer Aufgabe zu aktualisieren
        private async Task OnCheckIsChanged(TodoModel model)
        {
            if (await manager.OnUpdateTask(manager.ActiveUser.Id, model))
                await LoadTasksForSelectedTeam();
        }

        // Command für das Löschen einer Aufgabe
        [RelayCommand]
        private async Task OnDeleteTask(TodoModel task)
        {
            if (await manager.OnRemoveTask(task.Id, manager.ActiveUser.Id))
                await LoadTasksForSelectedTeam();
        }

        // Command zum Hinzufügen einer Aufgabe zu einem Team
        [RelayCommand]
        private async Task OnAddTaskToTeam()
        {
            if (SelectedTeam == null)
            {
                await Shell.Current.DisplayAlert("Fehler", "Es muss ein Team ausgewählt sein.", "Ok");
                return;
            }

            var action = await Shell.Current.DisplayPromptAsync("Aufgabenbenennung", "Bitte gebe deiner Aufgabe einen Namen...", "Speichern", "Abbrechen", "Dein Aufgabenname...");
            if (!string.IsNullOrEmpty(action))
            {
                TodoModel newTeamTask = new TodoModel
                {
                    Id = Guid.NewGuid().ToString(),
                    OwnerId = manager.ActiveUser.Id,
                    TeamId = SelectedTeam.TeamId,
                    Title = action,
                    IsChecked = false
                };

                if (await manager.OnAddTodo(newTeamTask))
                    await LoadTasksForSelectedTeam();
            }
        }

        partial void OnSelectedTeamChanged(TeamModel value)
        {
            Task.Run(async () => LoadTasksForSelectedTeam());
        }
    }
}
