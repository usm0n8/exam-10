using Application.Common;
using Application.DTOs.Users;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class UserService(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Result<List<GetUserDto>>> GetAllAsync()
    {
        var users = userManager.Users.ToList();
        var result = new List<GetUserDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new GetUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName!,
                Roles = roles.ToList()
            });
        }

        return Result<List<GetUserDto>>.Ok(result);
    }

    public async Task<Result<GetUserDto>> GetByIdAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            logger.LogWarning("User not found");
            return Result<GetUserDto>.Fail("User not found", ErrorType.NotFound);
        }

        var roles = await userManager.GetRolesAsync(user);
        return Result<GetUserDto>.Ok(new GetUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName!,
            Roles = roles.ToList()
        });
    }

    public async Task<Result<GetUserDto>> CreateAsync(CreateUserDto dto)
    {
        var existing = await userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
        {
            logger.LogWarning("Email already exists");
            return Result<GetUserDto>.Fail("Email already exists", ErrorType.Conflict);
        }

        if (!await roleManager.RoleExistsAsync(dto.Role))
        {
            logger.LogWarning("Role not found");
            return Result<GetUserDto>.Fail("Role not found", ErrorType.NotFound);
        }

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to create user");
            return Result<GetUserDto>.Fail("Failed to create user", ErrorType.Validation);
        }

        await userManager.AddToRoleAsync(user, dto.Role);

        var roles = await userManager.GetRolesAsync(user);
        return Result<GetUserDto>.Ok(new GetUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToList()
        });
    }

    public async Task<Result<GetUserDto>> UpdateAsync(string id, UpdateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            logger.LogWarning("User not found");
            return Result<GetUserDto>.Fail("User not found", ErrorType.NotFound);
        }

        if (dto.FullName != null)
        {
            user.FullName = dto.FullName;
        }
        if (dto.Email != null)
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            logger.LogWarning("Failed to update user");
            return Result<GetUserDto>.Fail("Failed to update the user", ErrorType.Validation);
        }
        if (dto.Role != null)
        {
            if (!await roleManager.RoleExistsAsync(dto.Role))
            {
                return Result<GetUserDto>.Fail("Role not found", ErrorType.NotFound);
            }
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, dto.Role);
        }

        var roles = await userManager.GetRolesAsync(user);
        return Result<GetUserDto>.Ok(new GetUserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName!,
            Roles = roles.ToList()
        });
    }

    public async Task<Result<bool>> DeleteAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            logger.LogWarning("User not found");
            return Result<bool>.Fail("User not found", ErrorType.NotFound);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to delete user");
            return Result<bool>.Fail("Failed to delete the user", ErrorType.Validation);
        }
        logger.LogInformation("User deleted");
        return Result<bool>.Ok(true);
    }
}