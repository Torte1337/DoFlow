using System;
using System.ComponentModel.DataAnnotations;

namespace DoFlow.Models;

public class TodoModel
{
    [Key]
    public Guid Id {get;set;}
    public string Name {get;set;}
    public bool IsDone {get;set;}
    public Guid UserId {get;set;}
}
