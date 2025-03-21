using System;
using System.Reactive.Linq;
using DoFlow.Models;
using Firebase.Database;

namespace DoFlow.Services;

public class FirebaseService
{
    private readonly FirebaseClient _client;

    public FirebaseService(FirebaseClient cl)
    {
        _client = cl;
    }

    public void OnSubscribeToTeamChanges(Action<TeamModel> onTeamAdded)
    {
        _client
        .Child("Teams")
        .AsObservable<TeamModel>()
        .Where(f => f.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
        .Subscribe(f => 
        {
            if(f.Object != null)
                onTeamAdded?.Invoke(f.Object);
        });
    }
}
