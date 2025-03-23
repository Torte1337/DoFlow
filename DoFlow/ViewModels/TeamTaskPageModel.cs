using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm;
using DoFlow.Manager;
using DoFlow.Models;
using DoFlow.Services;
using System.Linq;
using System.Threading.Tasks;
using DoFlow.ViewModels.ViewModelBase;
using System.ComponentModel;

namespace DoFlow.ViewModels
{
    public partial class TeamTaskPageModel : BaseViewModel
    {
        private readonly DatabaseManager manager;
        private readonly FirebaseService _firebaseService;

        [ObservableProperty]
        private ObservableCollection<TeamModel> teams;

        [ObservableProperty]
        private ObservableCollection<TodoModel> teamToDos;

        [ObservableProperty]
        private TeamModel selectedTeam;
        private IDisposable _taskSubscription;


        public TeamTaskPageModel(DatabaseManager mgr, FirebaseService service)
        {
            manager = mgr;
            _firebaseService = service;
            MainThread.BeginInvokeOnMainThread(() => Teams = new ObservableCollection<TeamModel>());
            MainThread.BeginInvokeOnMainThread(() => TeamToDos = new ObservableCollection<TodoModel>());
        }

        // Methode zum Laden der Teams
        public async Task LoadTeams()
        {
            try
            {
                if (Teams is {Count: > 0})
                    MainThread.BeginInvokeOnMainThread(() => Teams.Clear());

                var list = await manager.OnGetUserTeams();

                if(list == null)
                    return;
                    
                foreach (var item in list)
                    MainThread.BeginInvokeOnMainThread(() => Teams.Add(item));

                _firebaseService.OnSubscribeToTeamChanges(team =>
                {
                    if (!Teams.Any(t => t.Id == team.Id))
                    {
                        MainThread.BeginInvokeOnMainThread(() => Teams.Add(team));
                    }
                });

                if(Teams is {Count: > 0})
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        SelectedTeam = Teams.First();
                    });
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
                // Alte Subscription beenden
                _taskSubscription?.Dispose();

                if (TeamToDos.Count > 0)
                    MainThread.BeginInvokeOnMainThread(() => TeamToDos.Clear());

                if (SelectedTeam == null)
                    return;

                var list = await manager.OnGetTeamTasks(SelectedTeam.TeamId);

                if (list == null)
                {
                    await Shell.Current.DisplayAlert("Fehler", "Beim Laden ist ein Fehler aufgetreten", "Ok");
                    return;
                }

                foreach (var todo in list.OrderBy(x => x.IsChecked))
                {
                    todo.PropertyChanged += ToDo_PropertyChanged;
                    MainThread.BeginInvokeOnMainThread(() => TeamToDos.Add(todo));
                }

                // Live-Änderungen beobachten
                    _taskSubscription = _firebaseService.SubscribeToTeamTasks(
                    SelectedTeam.TeamId,
                    onTaskAddedOrChanged: (changedTask) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var existing = TeamToDos.FirstOrDefault(t => t.Id == changedTask.Id);
                            if (existing != null)
                            {
                                // Aktualisieren
                                var index = TeamToDos.IndexOf(existing);
                                
                                // Altes Event entfernen
                                existing.PropertyChanged -= ToDo_PropertyChanged;

                                // Neues Event hinzufügen
                                changedTask.PropertyChanged += ToDo_PropertyChanged;

                                TeamToDos[index] = changedTask;
                            }
                            else
                            {
                                // Neu hinzufügen
                                TeamToDos.Add(changedTask);
                            }
                        });
                    },
                    onTaskDeleted: (taskId) =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            var task = TeamToDos.FirstOrDefault(t => t.Id == taskId);
                            if (task != null)
                            {
                                task.PropertyChanged -= ToDo_PropertyChanged;
                                TeamToDos.Remove(task);
                            }
                        });
                    });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Fehler", ex.Message, "Ok");
            }
        }
        // PropertyChanged-Handler für Aufgaben, wenn "IsChecked" geändert wird 
        private async void ToDo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Property changed: {e.PropertyName}");
            if (sender is TodoModel todo && e.PropertyName == nameof(TodoModel.IsChecked))
            {
                await OnCheckIsChanged(todo);
            }
        }

        // Methode, um den Status einer Aufgabe zu aktualisieren
        private async Task OnCheckIsChanged(TodoModel model)
        {
            if (await manager.OnUpdateTeamTask(manager.ActiveUser.Id, model))
                await LoadTasksForSelectedTeam();
        }

        // Command für das Löschen einer Aufgabe
        [RelayCommand]
        private async Task OnDeleteTask(TodoModel task)
        {
            if (await manager.OnDeleteTeamTask(task))
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
