using LoyaltySystem.Application.Features.Auth.Commands;
using LoyaltySystem.Application.Features.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login(LoginCommand command)
    {
        return await _mediator.Send(command); // Sẽ gọi đến LoginCommandHandler
    }


    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var userId = await _mediator.Send(command);
        return Ok(new { Message = "Đăng ký thành công", UserId = userId });
    }

}