namespace WebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Authorization;
using WebApi.Helpers;
using WebApi.Models.Emails;
using WebApi.Models.Users;
using WebApi.Services;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UsersController> _logger;    
    private IUserService _userService;

    public UsersController(
        IEmailService emailService,
        ILogger<UsersController> logger,
        IUserService userService)
    {
        _emailService = emailService;
        _logger = logger;
        _userService = userService;
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
        var userToEmail = new Addressee
        {
            Name = $"{model.FirstName} {model.LastName}",
            Email = model.Email,
        };
        string subject = "Welcome to the User API!";
        string body = "Thank you for signing up!";
        var message = _emailService.Compose(userToEmail, subject, body, null);
        await _emailService.SendAsync(message);
        return Ok(new { message = "Registration successful! Check your email!" });
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