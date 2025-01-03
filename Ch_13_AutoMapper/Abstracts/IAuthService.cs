using Entities.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Abstracts;
public interface IAuthService
{
    Task<IdentityResult> ResigterUserAsync(UserDtoForRegistration userDto);
}