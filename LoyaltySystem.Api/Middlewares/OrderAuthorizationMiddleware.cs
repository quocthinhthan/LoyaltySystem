using System.Security.Claims;

namespace LoyaltySystem.Api.Middlewares;

public class OrderAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OrderAuthorizationMiddleware> _logger;

    public OrderAuthorizationMiddleware(RequestDelegate next, ILogger<OrderAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        var method = context.Request.Method;

        // Chỉ kiểm tra endpoint tạo Order (POST /api/order/create hoặc /api/orders)
        if (method == "POST" && (path?.Contains("/api/order") == true))
        {
            // 1. Kiểm tra user đã authenticate chưa
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning(
                    "Unauthorized access attempt to create order from IP: {IP}",
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "Bạn cần đăng nhập để tạo đơn hàng."
                });
                return;
            }

            // 2. Lấy thông tin user
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

            // 3. Kiểm tra role - Chỉ Staff và Admin được tạo order
            if (role != "Staff" && role != "Admin")
            {
                _logger.LogWarning(
                    "Forbidden: User {UserId} ({UserName}) with role '{Role}' attempted to create order",
                    userId, userName, role);

                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden",
                    message = $"Chỉ Staff và Admin mới có quyền tạo đơn hàng. Role hiện tại của bạn: {role}"
                });
                return;
            }

            // 4. Log successful authorization
            _logger.LogInformation(
                "Order creation authorized for User {UserId} ({UserName}) with role '{Role}'",
                userId, userName, role);
        }

        // Tiếp tục xử lý request
        await _next(context);
    }
}