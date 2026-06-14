namespace Application.DTOs.Users;

public class GetUserDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
    public List<string> Roles { get; set; } = null!;
}