using System;
using System.Reactive.Linq;
using DoFlow.Models;
using Firebase.Database;
using Firebase.Database.Query;

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
    public IDisposable SubscribeToTeamTasks(string teamId, Action<TodoModel> onTaskAddedOrChanged, Action<string> onTaskDeleted)
    {
        return _client
            .Child("Teams")
            .Child(teamId)
            .Child("Tasks")
            .AsObservable<TodoModel>()
            .Subscribe(taskEvent =>
            {
                switch (taskEvent.EventType)
                {
                    case Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate:
                        onTaskAddedOrChanged?.Invoke(taskEvent.Object);
                        break;
                    case Firebase.Database.Streaming.FirebaseEventType.Delete:
                        onTaskDeleted?.Invoke(taskEvent.Key);
                        break;
                }
            });
    }
}
