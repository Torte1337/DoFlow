using System;
using System.ComponentModel.DataAnnotations;

namespace DoFlow.Models;

public class TodoModel
{
    public string Id {get;set;}
    public string Name {get;set;}
    public bool IsDone {get;set;}
    public string AdminId {get;set;}
    public string TeamId {get;set;}
}
