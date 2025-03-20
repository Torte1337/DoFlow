using System;

namespace DoFlow.Models;

public class TeamModel
{
    public string Id { get; set; }
    public string TeamId { get; set; }
    public string Name { get; set; }
    public string AdminId { get; set; }
    public Dictionary<string,UserModel> Members { get;set;}
}
