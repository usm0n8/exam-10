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
    ILogger<UserService> logger,
    IMemoryCache memoryCacheService,
    IRedisCache redisCacheService) : IUserService
{
    public async Task<Result<List<GetUserDto>>> GetAllAsync()
    {
        const string cacheKey = "Users";
        var usersInCache = await redisCacheService.GetAsync<List<GetUserDto>>(cacheKey);

        if (usersInCache == null)
        {
            var users = userManager.Users.ToList();
            var userDto = users.Select(u => new GetUserDto
            {
                Id = u.Id,
                FullName = u.FullName!,
                Email = u.Email!
            }).ToList();
            await redisCacheService.SetAsync<List<GetUserDto>>(cacheKey,userDto,1);
            return  Result<List<GetUserDto>>.Ok(userDto);
        }
        
        return Result<List<GetUserDto>>.Ok(usersInCache);
    }

    public async Task<Result<GetUserDto>> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            logger.LogWarning("Id is required");
            return Result<GetUserDto>.Fail("Id is required", ErrorType.Validation);
        }        
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
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            logger.LogWarning("Email is required");
            return Result<GetUserDto>.Fail("Email is required", ErrorType.Validation);
        }
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            logger.LogWarning("Password is required");
            return Result<GetUserDto>.Fail("Password is required", ErrorType.Validation);
        }
        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            logger.LogWarning("FullName is required");
            return Result<GetUserDto>.Fail("FullName is required", ErrorType.Validation);
        }
        if (string.IsNullOrWhiteSpace(dto.Role))
        {
            logger.LogWarning("Role is required");
            return Result<GetUserDto>.Fail("Role is required", ErrorType.Validation);
        }
        if (dto.Role != UserRoles.Admin && dto.Role != UserRoles.Manager && dto.Role != UserRoles.User)
        {
            logger.LogWarning("Invalid role");
            return Result<GetUserDto>.Fail("Invalid role. Must be Admin, Manager or User", ErrorType.Validation);
        }        
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
        if (string.IsNullOrWhiteSpace(id))
        {
            logger.LogWarning("Id is required");
            return Result<GetUserDto>.Fail("Id is required", ErrorType.Validation);
        }
        if (dto.Email != null && !dto.Email.Contains("@"))
        {
            logger.LogWarning("Email is required");
            return Result<GetUserDto>.Fail("Invalid email format", ErrorType.Validation);
        }
        if (dto.Role != UserRoles.Admin && dto.Role != UserRoles.Manager && dto.Role != UserRoles.User)
        {
            logger.LogWarning("Invalid role");
            return Result<GetUserDto>.Fail("Invalid role. Must be Admin, Manager or User", ErrorType.Validation);
        }        
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
        if (string.IsNullOrWhiteSpace(id))
        {
            logger.LogWarning("Id is required");
            return Result<bool>.Fail("Id is required", ErrorType.Validation);
        }
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