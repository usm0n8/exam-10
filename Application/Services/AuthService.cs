using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AuthService(
    UserManager<User> userManager,
    IJwtTokenService tokenService,
    IEmailService emailService,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<Result<string>> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await userManager.FindByEmailAsync(dto.Email);
        if(existingUser != null)
        {
            logger.LogWarning("User already exists");
            return Result<string>.Fail("User already exists", ErrorType.NotFound);
        }

        var user = new User
        {
            UserName = dto.Email,
            FullName = dto.FullName,
            Email = dto.Email
        };
        var result = await userManager.CreateAsync(user, dto.Password);
        
        await userManager.AddToRoleAsync(user, UserRoles.User);
        logger.LogInformation("Registered successfully.");
        return Result<string>.Ok("Registered successfully");
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            logger.LogWarning("User not found");
            return Result<AuthResponseDto>.Fail("User not found", ErrorType.NotFound);
        }
        var result = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!result)
        {
            logger.LogError("Username or password is incorrect");
            return Result<AuthResponseDto>.Fail("Username or password is incorrect", ErrorType.Validation);
        }

        var role = await userManager.GetRolesAsync(user);
        var token = tokenService.GenerateToken(user);

        var response = new AuthResponseDto
        {
            User = new GetUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Roles = role.ToList()
            },
            Token = token  
        };
        logger.LogInformation("Logged in successfully");
        return Result<AuthResponseDto>.Ok(response);
    }

    public async Task<Result<string>> ChangePasswordAsync(string id, ChangePasswordDto dto)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            logger.LogWarning("User not found");
            return Result<string>.Fail("User not found", ErrorType.NotFound);
        }
        var result = await userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to change password");
            return Result<string>.Fail("Failed to change password", ErrorType.Validation);
        }
        logger.LogInformation("Password changed successfully");
        return Result<string>.Ok("Password changed successfully");
    }

    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null) 
        {
            logger.LogWarning("User not found");
            return Result<string>.Fail("User not found", ErrorType.NotFound);
        }
        var code = new Random().Next(100000, 999999).ToString();
        user.ResetCode = code;
        user.ResetCodeExpirest = DateTime.UtcNow.AddMinutes(3);
        await userManager.UpdateAsync(user);
        
        var sent = await emailService.SendEmailAsync(user.Email!, "Reset Code", $"Your code is: {code}");
        if (!sent)
        {
            logger.LogError("Failed to send email");
            return Result<string>.Fail("Failed to send email", ErrorType.Validation);
        }
        return Result<string>.Ok("Email sent successfully");
    }

    public async Task<Result<bool>> VerifyCodeAsync(VerifyCodeDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            logger.LogWarning("User not found");
            return Result<bool>.Fail("User not found", ErrorType.NotFound);
        }

        if (user.ResetCode != dto.Code || user.ResetCodeExpirest < DateTime.UtcNow)
        {
            logger.LogWarning("Invalid code");
            return Result<bool>.Fail("Invalid code", ErrorType.Validation);
        }
        
        return Result<bool>.Ok(true);
}

    public async Task<Result<string>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            logger.LogWarning("User not found");
            return Result<string>.Fail("User not found", ErrorType.NotFound);
        }
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, dto.Password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Reset password failed");
            return Result<string>.Fail("Reset pasword failed");
        }

        user.ResetCode = null!;
        user.ResetCodeExpirest = null;
        await  userManager.UpdateAsync(user);
        logger.LogInformation("Password reset successfully");
        return Result<string>.Ok("Password reset successfully");
    }
}