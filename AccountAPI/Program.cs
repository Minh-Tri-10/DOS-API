using AccountAPI.Repositories.Interfaces;
using AccountAPI.Repositories;
using AccountAPI.Services.Interfaces;
using AccountAPI.Services;
using AccountAPI.Models;
using AccountAPI.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AccountAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---- Đăng ký các dịch vụ nền tảng mà AccountAPI cần.
            builder.Services.AddMemoryCache();
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(AccountAPI.Mapping.MappingProfile));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DbContext kết nối tới database chứa bảng Users.
            builder.Services.AddDbContext<AccountDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("HuyConnection")));

               


            // EmailOptions phục vụ gửi token reset/ thông báo tới user.
            builder.Services.AddSingleton(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>().GetSection("Email");
                return new EmailOptions
                {
                    ServiceId = cfg["ServiceId"]!,
                    TemplateId = cfg["TemplateId"]!,
                    PublicKey = cfg["PublicKey"]!,
                    AccessToken = cfg["AccessToken"],
                    FromName = cfg["FromName"] ?? "DrinkOrder Support (DEV)",
                    Origin = cfg["Origin"] ?? builder.Configuration["Frontend:BaseUrl"] ?? "https://localhost"
                };
            });
            builder.Services.AddSingleton<IEmailSender, EmailJsSender>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

            // ---- Cấu hình xác thực JWT cho toàn bộ API endpoints.
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var signingKey = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException("JWT:Key configuration is missing.");
            }

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError(context.Exception, "JWT authentication failed for {Path}", context.HttpContext.Request.Path);
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogWarning("JWT challenge triggered for {Path}. Error: {Error}, Description: {Description}", context.HttpContext.Request.Path, context.Error, context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Chuỗi middleware chuẩn cho Web API có JWT.
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
