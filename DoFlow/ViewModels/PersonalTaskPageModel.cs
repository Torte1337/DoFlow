using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DoFlow.Manager;
using DoFlow.Messages;
using DoFlow.Models;
using DoFlow.ViewModels.ViewModelBase;

namespace DoFlow.ViewModels;

public partial class PersonalTaskPageModel : BaseViewModel
{
    private readonly DatabaseManager manager;
    [ObservableProperty] private ObservableCollection<TodoModel> personalTasks = new ObservableCollection<TodoModel>();

    public PersonalTaskPageModel(DatabaseManager mgr)
    {
        manager = mgr;

        OnLoadTasks();
        WeakReferenceMessenger.Default.Register<MessageUserDeleted>(this,OnUserWasDeleted);
    }
    private void OnUserWasDeleted(object recipient, MessageUserDeleted msg)
    {
        PersonalTasks.Clear();
    }

    private async void OnLoadTasks()
    {
        if(PersonalTasks is {Count: > 0})
            PersonalTasks.Clear();

        var list = await manager.OnGetPersonalTasks(manager.ActiveUser.Id);

        var sortedList = list.OrderBy(x => x.IsChecked).ToList();
        PersonalTasks = new ObservableCollection<TodoModel>(sortedList);

    }
    public async Task OnCheckIsChanged(TodoModel model)
    {
        if(await manager.OnUpdateTask(manager.ActiveUser.Id,model))
            OnLoadTasks();
    }
    [RelayCommand]
    private async Task OnDeleteTask(TodoModel task)
    {
        if(await manager.OnRemoveTask(task.Id,manager.ActiveUser.Id))
            OnLoadTasks();
    }
    [RelayCommand]
    private async Task OnAddPersonalTask()
    {
        var action = await Shell.Current.DisplayPromptAsync("Aufgabenbenennung", "Bitte gebe deiner Aufgabe einen Namen...","Speichern","Abbrechen","Dein Aufgabenname...");
        if(!string.IsNullOrEmpty(action))
        {
            TodoModel personalTask = new TodoModel
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = manager.ActiveUser.Id,
                TeamId = null,
                Title = action,
                IsChecked = false
            };

            if(await manager.OnAddTodo(personalTask))
                OnLoadTasks();
        }
    }
}
