namespace DoFlow.Models;

public class UserModel
{
    public string Id { get; set; }
    public string Username {get;set;}
    public List<TodoModel> personalTasks {get;set;} = new();
}
