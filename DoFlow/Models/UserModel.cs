namespace DoFlow.Models;

public class UserModel
{
    public string Id { get; set; }
    public string Email {get;set;}
    public string EmailSalt {get;set;}
    public string Username {get;set;}
    public string Password { get; set; }
    public string PasswordSalt {get;set;}

}
