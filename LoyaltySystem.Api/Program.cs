using LoyaltySystem.Api.Middlewares;
using LoyaltySystem.Application;
using LoyaltySystem.Application.Common.Interfaces;
using LoyaltySystem.Application.Features.Auth.Commands;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Infrastructure;
using LoyaltySystem.Infrastructure.Repositories;
using LoyaltySystem.Infrastructure.Services; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection.PortableExecutable;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// 1. Cấu hình Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký các Interface của hệ thống 
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



// 5. Cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "Key_Bi_Mat_Toi_Thieu_32_Ky_Tu_Nhe_Thinh";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "LoyaltySystem",
        ValidAudience = jwtSettings["Audience"] ?? "LoyaltySystem",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// 6. AUTHORIZATION POLICIESs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
});

// 7. Cấu hình Swagger với nút Authorize
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
app.UseAuthentication();
app.UseWhen(context => context.Request.Path.StartsWithSegments("/api/Orders"), appBuilder =>
{
    appBuilder.UseMiddleware<OrderAuthorizationMiddleware>();
});
app.UseAuthorization();
    
app.MapControllers();

app.Run();