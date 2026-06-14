using Application.Common;
using Application.DTOs.Users;

namespace Application.Interfaces;

public interface IUserService
{
    Task<Result<List<GetUserDto>>> GetAllAsync();
    Task<Result<GetUserDto>> GetByIdAsync(string id);
    Task<Result<GetUserDto>> CreateAsync(CreateUserDto dto);
    Task<Result<GetUserDto>> UpdateAsync(string id, UpdateUserDto dto);
    Task<Result<bool>> DeleteAsync(string id);
}