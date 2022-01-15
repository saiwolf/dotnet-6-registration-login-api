namespace WebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApi.Authorization;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using BCryptNet = BCrypt.Net.BCrypt;

public interface IUserService
{
    Task<AuthenticateResponse> Authenticate(AuthenticateRequest model);
    IEnumerable<User> GetAll();
    Task<User> GetById(Guid id);
    Task Register(RegisterRequest model);
    Task Update(Guid id, UpdateRequest model);
    Task Delete(Guid id);
}

public class UserService : IUserService
{
    private DataContext _context;
    private IJwtUtils _jwtUtils;
    private ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(
        DataContext context,
        IJwtUtils jwtUtils,
        ILogger<UserService> logger,
        IMapper mapper)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
    {
        try
        {
            if (model.Password != model.ConfirmPassword)
                throw new AppException("Password and Confirmation do not match.");

            User user = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

            // validate
            if (user == null || !BCryptNet.Verify(model.Password, user.PasswordHash))
                throw new AppException("Email or password is incorrect");

            // authentication successful
            var response = _mapper.Map<AuthenticateResponse>(user);
            response.Token = _jwtUtils.GenerateToken(user);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public IEnumerable<User> GetAll()
    {
        try
        {
            return _context.Users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task<User> GetById(Guid id)
    {
        return await getUserAsync(id);
    }

    public async Task Register(RegisterRequest model)
    {
        try
        {
            if (model.Password != model.ConfirmPassword)
                throw new AppException("Password and Confirmation do not match.");

            // validate
            if (_context.Users.Any(x => x.Email == model.Email))
                throw new AppException("Email '" + model.Email + "' is already taken");

            // map model to new user object
            var user = _mapper.Map<User>(model);

            user.Created = DateTime.Now;
            user.Updated = user.Created;

            user.Id = Guid.NewGuid();

            // hash password
            user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // save user
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task Update(Guid id, UpdateRequest model)
    {
        try
        {
            if (model.Password != model.ConfirmPassword)
                throw new AppException("Password and Confirmation do not match.");

            var user = await getUserAsync(id);

            // validate
            if (model.Email != user.Email && _context.Users.Any(x => x.Email == model.Email))
                throw new AppException("Email '" + model.Email + "' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = BCryptNet.HashPassword(model.Password);

            // copy model to user and save
            _mapper.Map(model, user);
            user.Updated = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public async Task Delete(Guid id)
    {
        try
        {
            var user = await getUserAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    // helper methods

    private async Task<User> getUserAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return user;
    }
}