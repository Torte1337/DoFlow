using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DoFlow.Context;
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
    
    
    #endregion

    #region Todos Team
    
    #endregion

    #region Team Methods

    #endregion

    #region Todos Personal
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
                //Hier wird nicht im User der Task gespeichert sondern im Team
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
    #endregion




}