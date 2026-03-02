using MediatR;
using Microsoft.AspNetCore.Mvc;
using LoyaltySystem.Application.Features.Auth.Commands;


namespace LoyaltySystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var userId = await _mediator.Send(command);
            return Ok(new { Message = "Đăng ký thành công", UserId = userId });
        }
    }
}
