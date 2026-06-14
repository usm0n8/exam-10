using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser
{
    public string? FullName { get; set; } 
    public string? ResetCode { get; set; } 
    public DateTime? ResetCodeExpirest { get; set; }
}