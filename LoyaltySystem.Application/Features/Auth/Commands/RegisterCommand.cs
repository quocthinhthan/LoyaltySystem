using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace LoyaltySystem.Application.Features.Auth.Commands
{
    // Yêu cầu đăng ký trả về UserId (int) sau khi thành công
    public record RegisterCommand(string UserName, string PhoneNumber, string Password, string Role) : IRequest<int>;
}
