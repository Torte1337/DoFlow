using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoFlow.Manager;
using DoFlow.ViewModels.ViewModelBase;
using DoFlow.Views;
using DoFlow.Views.ContentViews;

namespace DoFlow.ViewModels;

public partial class AuthPageModel : BaseViewModel
{
    private readonly DatabaseManager dbManager;
    private readonly IServiceProvider _serviceProvider;

    #region Propertys Login
    [ObservableProperty] private string emailField = "";
    [ObservableProperty] private string passwordField = "";
    [ObservableProperty] private bool loginViewActive = false;
    #endregion
    #region Propertys Register
    [ObservableProperty] private string registerEmailField = "";
    [ObservableProperty] private string registerPasswordField = "";
    [ObservableProperty] private string registerUsernameField = "";
    [ObservableProperty] private bool registerViewActive = false;
    #endregion
    #region Propertys Lostpassword
    [ObservableProperty] private string lostEmailField = "";
    [ObservableProperty] private bool lostViewActive = false;
    #endregion
    #region Propertys Views
    [ObservableProperty] private SignInView signInView;
    [ObservableProperty] private LostPasswordView lostView;
    [ObservableProperty] private RegisterView registerView;
    #endregion
    public AuthPageModel(DatabaseManager mgr,IServiceProvider serviceProvider)
    {
        if(dbManager == null)
            dbManager = mgr;

        _serviceProvider = serviceProvider;


        SignInView = _serviceProvider.GetRequiredService<SignInView>();
        LostView = _serviceProvider.GetRequiredService<LostPasswordView>();
        RegisterView = _serviceProvider.GetRequiredService<RegisterView>();

        OnCheckUserExist();
    }
    private async void OnCheckUserExist()
    {
        if(await dbManager.OnCheckPresetExist())
        {
            await Shell.Current.GoToAsync($"//{nameof(PersonalTaskPage)}");
            EmailField = "";
            PasswordField = "";
        }
        else
        {
            OnSwitchViews(0);
        }
    }

    [RelayCommand]
    private async Task OnSignIn()
    {
        if(await dbManager.OnSignIn(EmailField,PasswordField))
        {
            var resultAction = await Shell.Current.DisplayActionSheet("Anmeldedaten speichern ?","Nein",null,"Ja");
            if(resultAction == "Ja")
            {
                if(await dbManager.OnSavePreset(EmailField,PasswordField))
                {
                    await Shell.Current.DisplayAlert("Info","Daten wurden gespeichert","Ok");
                }
            }
            await Shell.Current.GoToAsync($"//{nameof(PersonalTaskPage)}");
            EmailField = "";
            PasswordField = "";
        }
    }
    [RelayCommand]
    private async Task OnGoToLostPassword()
    {
        OnSwitchViews(2);
    }
    [RelayCommand]
    private async Task OnGoToRegister()
    {
        OnSwitchViews(1);
    }
    [RelayCommand]
    private async Task OnLostPassword()
    {
        await dbManager.OnLostpassword(LostEmailField);
        await dbManager.OnClearPresets();
        OnSwitchViews(0);
    }
    [RelayCommand]
    private async Task OnRegister()
    {
        await dbManager.OnRegister(RegisterEmailField, RegisterPasswordField,RegisterUsernameField);
        OnSwitchViews(0);
    }
    [RelayCommand]
    private async Task OnBackButton()
    {
        OnSwitchViews(0);
    }
    
    private void OnSwitchViews(int viewID)
    {
        OnClearFields();
        switch(viewID)
        {
            case 0:
                LoginViewActive = true;
                RegisterViewActive = false;
                LostViewActive = false;
            break;

            case 1:
                LoginViewActive = false;
                RegisterViewActive = true;
                LostViewActive = false;
            break;

            case 2:
                LoginViewActive = false;
                RegisterViewActive = false;
                LostViewActive = true;
            break;
        }
    }
    private void OnClearFields()
    {
        EmailField = "";
        PasswordField = "";
        RegisterEmailField = "";
        RegisterPasswordField = "";
        RegisterUsernameField = "";
        LostEmailField = "";
    }


}
