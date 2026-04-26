using System;

namespace GameEngineLab.Core.Features.Identity.Resources;

public sealed class UserAccountResource
{
    public Guid UserId { get; set; } = Guid.Empty;
    
    public string Username { get; set; } = "Guest";
    
    public string AuthToken { get; set; } = string.Empty;
    
    public bool IsLoggedIn => UserId != Guid.Empty;

    public void Login(string username)
    {
        Username = username;
        UserId = Guid.NewGuid();
        AuthToken = $"token_{UserId}";
    }
}
