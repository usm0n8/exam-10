using Application.DTOs.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }       
        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }        
        return Ok(result.Data);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized("User not found in token");

        var result = await authService.ChangePasswordAsync(userId, dto);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }        
        return Ok(result.Data);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await authService.ForgotPasswordAsync(dto);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }        
        return Ok(result.Data);
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeDto dto)
    {
        var result = await authService.VerifyCodeAsync(dto);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }        
        return Ok(result.Data);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = await authService.ResetPasswordAsync(dto);
        if (!result.IsSuccess)
        {
            return HandleError(result);
        }        
        return Ok(result.Data);
    }
}