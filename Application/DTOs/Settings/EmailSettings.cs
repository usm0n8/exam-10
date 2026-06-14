namespace Application.DTOs.Settings;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "";
    public int Port { get; set; }
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}