using Application.Common;
using Application.DTOs.Auth;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<Result<string>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<string>> ChangePasswordAsync(string id, ChangePasswordDto dto);
    Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<Result<bool>> VerifyCodeAsync(VerifyCodeDto dto);
    Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto);
}