using System;

namespace DoFlow.Models;

public class TeamModel
{
    public string Id { get; set; }
    public string TeamId { get; set; }
    public string Name { get; set; }
    public string AdminId { get; set; }
    public List<UserModel> Members { get; set; }
}
