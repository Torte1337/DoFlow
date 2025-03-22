using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DoFlow.Context;
using DoFlow.Messages;
using DoFlow.Models;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;

namespace DoFlow.Manager;

public partial class DatabaseManager : ObservableObject
{
    private readonly FirebaseAuthClient authClient;
    private readonly FirebaseClient _client;
    [ObservableProperty] private UserModel activeUser = null;
    
    [ObservableProperty] private Dictionary<string, IDisposable> _teamTaskSubscriptions = new();

    public DatabaseManager(FirebaseAuthClient client, FirebaseClient cl)
    {
        authClient = client;
        _client = cl;
    }
    
    #region Usermethoden
    /// <summary>
    /// Methode loggt den User ein (Auth)
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> OnSignIn(string email, string password)
    {
        try
        {
            var userDetails = await authClient.SignInWithEmailAndPasswordAsync(email,password);

            if(userDetails == null)
            {
                await Shell.Current.DisplayAlert("Fehler","User konnte nicht gefunden werden.","Ok");
                return false;
            }

            UserModel user = new UserModel
            {
                Id = userDetails.User.Uid,
                Username = userDetails.User.Info.DisplayName
            };

            ActiveUser = user;

            await OnCreatePersonalTeam();

            return true;
        }
        catch(Exception ex)
        {
            await Shell.Current.DisplayAlert("Fehler","Email oder Passwort stimmen nicht überein!","Ok");
            return false;
        }
    }
    /// <summary>
    /// Methode registriert einen User 
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<bool> OnRegister(string email, string password, string username)
    {
        try
        {
            await authClient.CreateUserWithEmailAndPasswordAsync(email,password,username);
            await Shell.Current.DisplayAlert("Info","User wurde angelegt","Ok");
            return true;
        }
        catch(Exception ex)
        {
            await Shell.Current.DisplayAlert("Fehler","User konnte nicht angelegt werden, bitte versuche es später noch einmal.","Ok");
            return false;
        }
    }
    /// <summary>
    /// Methode setzt das kennwort zurück
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<bool> OnLostpassword(string email)
    {
        try
        {
            await authClient.ResetEmailPasswordAsync(email);
            await Shell.Current.DisplayAlert("Info","Bitte prüfe dein Postfach, es wurde eine Email an deine Emailadresse geschickt","Ok");
            return true;
        }
        catch(Exception ex)
        {
            await Shell.Current.DisplayAlert("Fehler","Bitte versuche es später noch einmal","Ok");
            return false;
        }
    }
    /// <summary>
    /// Methode loggt den User aus / meldet ihn ab
    /// </summary>
    /// <returns></returns>
    public bool OnLogout()
    {
        try
        {
            authClient.SignOut();
            ActiveUser = null;
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    /// <summary>
    /// Methode ändert den Usernamen
    /// </summary>
    /// <param name="newName"></param>
    /// <returns></returns>
    public async Task<bool> OnChangeUsername(string newName)
    {
        try
        {
            await authClient.User.ChangeDisplayNameAsync(newName);
            return true;
        }
        catch(Exception ex)
        {
            await Shell.Current.DisplayAlert("Fehler","Bitte versuche es später noch einmal","Ok");
            return false;
        }
    }
    /// <summary>
    /// Methode ändert das bisherige Password mit dem neuen
    /// </summary>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public async Task<bool> OnChangePassword(string newPassword)
    {
        try
        {
            await authClient.User.ChangePasswordAsync(newPassword);
            return true;
        }
        catch(Exception ex)
        {
            await Shell.Current.DisplayAlert("Fehler","Bitte versuche es später noch einmal","Ok");
            return false;
        }
    }
    /// <summary>
    /// Methode legt den user in der Rt Db an
    /// </summary>
    /// <returns></returns>
    public async Task<bool> OnCreatePersonalTeam()
    {
        try
        {
            var userList = await _client.Child("Users").OnceAsync<UserModel>();

            var userExist = userList.Any(u => u.Object.Id == ActiveUser.Id);

            if(!userExist)
                await _client.Child("Users").Child(ActiveUser.Id).PutAsync(ActiveUser);

            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> OnDeleteAccount()
    {
        try
        {
            await _client.Child("Users").Child(ActiveUser.Id).DeleteAsync();

            var allTeams = await _client.Child("Teams").OnceAsync<TeamModel>();

            foreach(var team in allTeams)
            {
                if(team.Object.MemberIds.Contains(ActiveUser.Id))
                {
                    if(team.Object.AdminId == ActiveUser.Id)
                        await OnLeaveTeam(team.Object.TeamId);
                    else
                    {
                        team.Object.MemberIds.Remove(ActiveUser.Id);
                        await _client.Child("Teams").Child(team.Object.TeamId).Child("MemberIds").PutAsync(team.Object.MemberIds);
                    }
                }
            }

            await authClient.User.DeleteAsync();
            OnLogout();
            WeakReferenceMessenger.Default.Send(new MessageUserDeleted());
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    
    #endregion

    #region Team Methods
    public async Task<bool> OnCreateNewTeam(TeamModel model)
    {
        try
        {
            await _client.Child("Teams").Child(model.TeamId).PutAsync(model);
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> OnJoinTeam(string teamId)
    {
        try
        {
            // Teamdaten abrufen
            var teamSnapshot = await _client.Child("Teams").Child(teamId).OnceSingleAsync<dynamic>();
            if (teamSnapshot == null)
                return false;

            // MemberIds extrahieren
            List<string> memberIds = teamSnapshot.MemberIds?.ToObject<List<string>>() ?? new List<string>();

            // Prüfen, ob User schon Mitglied ist
            if (!memberIds.Contains(ActiveUser.Id))
            {
                memberIds.Add(ActiveUser.Id);
                // Nur das MemberIds-Feld aktualisieren
                await _client.Child("Teams").Child(teamId).Child("MemberIds").PutAsync(memberIds);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Team-Join: {ex.Message}");
            return false;
        }
    }
    public async Task<bool> OnLeaveTeam(string teamId)
    {
        try
        {
            // Teamdaten abrufen
            var teamSnapshot = await _client.Child("Teams").Child(teamId).OnceSingleAsync<dynamic>();
            if (teamSnapshot == null)
                return false;

            string adminId = teamSnapshot.AdminId;
            var memberIds = teamSnapshot.MemberIds?.ToObject<List<string>>() ?? new List<string>();

            // Falls der aktuelle User Admin ist → Team löschen
            if (adminId == ActiveUser.Id)
            {
                await _client.Child("Teams").Child(teamId).DeleteAsync();
                Console.WriteLine($"Team {teamId} gelöscht, da Admin ({ActiveUser.Id}) es verlassen hat.");
                return true;
            }

            // Falls User Mitglied ist → entfernen und speichern
            if (memberIds.Contains(ActiveUser.Id))
            {
                memberIds.Remove(ActiveUser.Id);
                await _client.Child("Teams").Child(teamId).Child("MemberIds").PutAsync(memberIds);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Verlassen des Teams: {ex.Message}");
            return false;
        }
    }
    public async Task<List<TeamModel>> OnGetUserTeams()
    {
        try
        {
            var allTeamsSnapshot = await _client.Child("Teams").OnceAsync<dynamic>();
            var userTeams = new List<TeamModel>();

            foreach (var item in allTeamsSnapshot)
            {
                if (item.Object == null || item.Object.MemberIds == null)
                    continue;

                // Prüfen, ob der aktuelle User im Team ist
                List<string> userList = item.Object.MemberIds.ToObject<List<string>>();
                if (!userList.Contains(ActiveUser.Id))
                    continue;

                // Team erstellen
                TeamModel newTeam = new TeamModel
                {
                    TeamId = item.Key,
                    Name = item.Object.Name,
                    AdminId = item.Object.AdminId,
                    MemberIds = userList,
                    Tasks = new List<TodoModel>()
                };

                // Tasks übernehmen, falls vorhanden
                if (item.Object.Tasks != null)
                {
                    foreach (var taskItem in item.Object.Tasks)
                    {
                        TodoModel task = new TodoModel
                        {
                            Id = taskItem.Value.Id,
                            Title = taskItem.Value.Title,
                            OwnerId = taskItem.Value.OwnerId,
                            TeamId = taskItem.Value.TeamId,
                            IsChecked = taskItem.Value.isChecked ?? false
                        };
                        newTeam.Tasks.Add(task);
                    }
                }

                userTeams.Add(newTeam);
            }

            return userTeams;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Abrufen der Teams: {ex.Message}");
            return null;
        }
    }

    #endregion

    #region Todos Personal/Team
    public async Task<bool> OnAddTodo(TodoModel newTodo)
    {
        try
        {
            if(string.IsNullOrEmpty(newTodo.TeamId))
            {
                //Personal Tasks
                await _client.Child("Users").Child(ActiveUser.Id).Child("Tasks").Child(newTodo.Id).PutAsync(newTodo);
                return true;
            }
            else if(!string.IsNullOrEmpty(newTodo.TeamId))
            {
                //Team Task
                await _client.Child("Teams").Child(newTodo.TeamId).Child("Tasks").Child(newTodo.Id).PutAsync(newTodo);
                return true;
            }
            else
                return false;

        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public async Task<List<TodoModel>> OnGetPersonalTasks(string userId)
    {
        try
        {
            var tasks = await _client.Child("Users")
                                     .Child(userId)
                                     .Child("Tasks")
                                     .OnceAsync<TodoModel>();

            var result = tasks.Select(x => x.Object).ToList();

            return result;
        }
        catch(Exception ex)
        {
            return null;
        }
    }
    public async Task<List<TodoModel>> OnGetTeamTasks(string teamId)
    {
        try
        {
            //Hier kann der Fehler sein, dass die App abstürtzt
            var tasks = await _client.Child("Teams")
                                     .Child(teamId)
                                     .Child("Tasks")
                                     .OnceAsync<dynamic>();


            var result = tasks.Select(item =>
            {
                TodoModel task = new TodoModel();
                task.Id = item.Object.Id;
                task.Title = item.Object.Title;
                task.OwnerId = item.Object.OwnerId;
                task.TeamId = item.Object.TeamId;
                task.IsChecked = item.Object.IsChecked;
                
                return task;
            }).ToList();

            return result;
        }
        catch(Exception ex)
        {
            await Shell.Current.DisplayAlert("Fehler", ex.Message + " DB Fehler Aufgaben vom Team holen","Ok");
            return null;
        }
    }
    public async Task<bool> OnRemoveTask(string taskId, string userId)
    {
        try
        {
            
            await _client.Child("Users").Child(userId).Child("Tasks").Child(taskId).DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> OnUpdateTask(string userId, TodoModel updatedModel)
    {
        try
        {
            await _client.Child("Users").Child(userId).Child("Tasks").Child(updatedModel.Id).PutAsync(updatedModel);

            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> OnDeleteTeamTask(TodoModel model)
    {
        try
        {
            await _client.Child("Teams").Child(model.TeamId).Child("Tasks").Child(model.Id).DeleteAsync();
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    #endregion

}