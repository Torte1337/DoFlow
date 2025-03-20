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
    
    
    
    #endregion
    #region Teammethoden
    /// <summary>
    /// Methode fügt ein Team in die Datenbank hinzu
    /// </summary>
    /// <param name="newTeam"></param>
    /// <returns></returns>
    public async Task<bool> OnAddTeamToDatabase(TeamModel newTeam, UserModel newMember)
    {
        try
        {
            await _client.Child("Teams").Child(newTeam.TeamId).PutAsync(newTeam);
            await OnJoinTeam(newTeam.TeamId,newMember);
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    /// <summary>
    /// Methode löscht ein Team aus der Datenbank
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public async Task<bool> OnDeleteTeam(TeamModel team)
    {
        try
        {
            await _client.Child("Teams").Child(team.TeamId).DeleteAsync();
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    /// <summary>
    /// Methode updated ein existierendes Team
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public async Task<bool> OnUpdateTeam(TeamModel team)
    {
        try
        {
            await _client.Child("Teams").Child(team.TeamId).PutAsync(team);
            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> OnSearchTeam(string Id)
    {
        try
        {
            var teamQuery = await _client.Child("Teams").Child(Id).OnceSingleAsync<TeamModel>();

            if(teamQuery != null)
                return true;
            else
                return false;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> OnJoinTeam(string teamID, UserModel user)
    {
        try
        {
            await _client
                .Child("Teams")
                .Child(teamID)
                .Child("Members")
                .Child(user.Id)
                .PutAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
            return false;
        }
    }
    public async Task<List<TeamModel>> OnGetMyTeams(string userID)
    {
        try
        {
            var allTeams = await _client.Child("Teams").OnceAsync<TeamModel>();
            var userTeams = new List<TeamModel>();

            foreach (var team in allTeams)
            {
                var teamData = team.Object;

                // Prüfen, ob der Benutzer in den Members enthalten ist
                if (teamData.Members != null && teamData.Members.ContainsKey(userID))
                {
                    userTeams.Add(teamData);
                }
            }

            return userTeams;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
            return null;
        }
    }
    public async Task<bool> OnLeaveTeam(string teamId, string userId)
    {
        try
        {
            await _client
            .Child("Teams")
            .Child(teamId)
            .Child("Members")
            .Child(userId)
            .DeleteAsync();

            return true;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
    #endregion





}