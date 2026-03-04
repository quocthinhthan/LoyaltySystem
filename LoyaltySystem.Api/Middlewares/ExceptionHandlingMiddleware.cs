using System.Net;
using System.Text.Json;
using FluentValidation;
using System.Text.Encodings.Web; // Thêm dòng này
using System.Text.Unicode;

namespace LoyaltySystem.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        object responseData; // Đổi từ string sang object để tránh serialize 2 lần

        if (exception is ValidationException validationException)
        {
            code = HttpStatusCode.BadRequest;
            // Lấy danh sách lỗi từ Validator
            responseData = new { error = validationException.Errors.Select(e => e.ErrorMessage) };
        }
        else if (exception is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
            responseData = new { error = exception.Message };
        }
        else
        {
            responseData = new { error = exception.Message };
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        // Cấu hình để hiển thị tiếng Việt có dấu
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Giúp key trả về dạng camelCase chuẩn
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(responseData, options));
    }
}