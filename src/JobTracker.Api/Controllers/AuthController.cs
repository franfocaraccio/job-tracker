using System.ComponentModel.DataAnnotations;
using JobTracker.Api.Contracts;
using JobTracker.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userService.RegisterAsync(request.Name, request.Email, request.Password, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new UserResponse(user.Id, user.Name, user.Email));
        }
        catch (ValidationException exception)
        {
            return Problem(detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var token = await _userService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (token is null)
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse(token));
    }
}
