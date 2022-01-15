namespace WebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Services;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private IUserService _userService;
    private IMapper _mapper;
    private readonly AppSettings _appSettings;

    public UsersController(
        IUserService userService,
        IMapper mapper,
        IOptions<AppSettings> appSettings)
    {
        _userService = userService;
        _mapper = mapper;
        _appSettings = appSettings.Value;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate(AuthenticateRequest model)
    {
        AuthenticateResponse response = await _userService.Authenticate(model);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        await _userService.Register(model);
        return Ok(new { message = "Registration successful" });
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var users = _userService.GetAll();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var user = await _userService.GetById(id);
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateRequest model)
    {
        await _userService.Update(id, model);
        return Ok(new { message = "User updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _userService.Delete(id);
        return Ok(new { message = "User deleted successfully" });
    }
}