using Abstracts;
using AutoMapper;
using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Services;
public class AuthenticationManager : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public AuthenticationManager(UserManager<User> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<IdentityResult> ResigterUserAsync(UserDtoForRegistration userDto)
    {
        var user = _mapper.Map<User>(userDto);
        var result = await _userManager.CreateAsync(user, userDto.Password);
        if(result.Succeeded)
        {
             await _userManager.AddToRolesAsync(user, userDto.Roles);
        }
        return result;
    }
}