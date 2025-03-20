using CommunityToolkit.Maui;
using DoFlow.Manager;
using Microsoft.Extensions.Logging;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using DoFlow.Views;
using DoFlow.ViewModels;
using DoFlow.Views.ContentViews;

namespace DoFlow;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});


		#if ANDROID
		Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline",(h,v) => 
		{
			h.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
		});
		#endif

		#region Firebase AuthClient
		builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig()
		{
			ApiKey = "AIzaSyAbiz1CcE5Y7lLTySjDKMOEUvY-s_10D50",
			AuthDomain = "doflow-363ba.firebaseapp.com",
			Providers = new FirebaseAuthProvider[]
			{
				new EmailProvider()
			}
		}));
		#endregion
		#region Firebase Database
		var firebaseDatabaseUrl = "https://doflow-363ba-default-rtdb.europe-west1.firebasedatabase.app/";
		#endregion
		
		builder.Services.AddSingleton(new FirebaseClient(firebaseDatabaseUrl));
		builder.Services.AddSingleton<DatabaseManager>();
		builder.Services.AddSingleton<AuthPageModel>();
		builder.Services.AddSingleton<SettingsPageModel>();

		builder.Services.AddTransient<AuthPage>();
		builder.Services.AddTransient<SignInView>();
		builder.Services.AddTransient<RegisterView>();
		builder.Services.AddTransient<LostPasswordView>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<DashboardPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
