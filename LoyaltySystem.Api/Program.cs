using LoyaltySystem.Application;
using LoyaltySystem.Application.Common.Interfaces;
using LoyaltySystem.Application.Features.Auth.Commands;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Infrastructure;
using LoyaltySystem.Infrastructure.Repositories;
using LoyaltySystem.Infrastructure.Services; 
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LoyaltySystem.Api.Middlewares; // Thêm dòng này

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký các Interface của hệ thống 
// Đăng ký IAppDbContext trỏ về ApplicationDbContext
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

// Đăng ký UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Đăng ký dịch vụ sinh Token cho Login
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Hỗ trợ lấy thông tin User từ HttpContext (cần cho CurrentUserService)
builder.Services.AddHttpContextAccessor();

// 3. Đăng ký MediatR để quét các Handler
//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
builder.Services.AddApplicationServices();

// 4. Các dịch vụ mặc định của API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 5. Cấu hình Swagger với nút Authorize
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Loyalty System API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Token theo định dạng: Bearer {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Lưu ý: Cần thêm UseAuthentication trước UseAuthorization để nhận diện Token
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();