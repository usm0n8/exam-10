using Application.DTOs.Users;

namespace Application.DTOs.Auth;

public class AuthResponseDto
{
    public GetUserDto User { get; set; } = null!;
    public string Token { get; set; } = null!;
}