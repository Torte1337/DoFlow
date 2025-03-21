using System;
using System.Collections.ObjectModel;

namespace DoFlow.Models;

public class TeamModel
{
    public string Id { get; set; }
    public string TeamId { get; set; }
    public string Name { get; set; }
    public string AdminId { get; set; }
    public List<string> MemberIds {get;set;} = new();
    public List<TodoModel> Tasks {get;set;} = new();
    
}
