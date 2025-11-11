using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace APIGateways
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Gateway đọc cấu hình tuyến từ Ocelot.json (reload khi file thay đổi).
            builder.Configuration.AddJsonFile("Ocelot.json", optional: false, reloadOnChange: true);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddOcelot(); // Đăng ký pipeline của Ocelot.

            // Tái sử dụng cùng cấu hình JWT như các service phía sau để kiểm tra token ngay tại cửa ngõ.
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var signingKey = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException("JWT:Key configuration is missing for API Gateway.");
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
                });

            builder.Services.AddAuthorization();

            // CORS mặc định để các FE clients (MVC/SPA) có thể gọi Gateway trong DEV.
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.UseOcelot(); // Bắt đầu pipeline reverse-proxy để forward request tới các microservice.

            app.Run();
        }
    }
}
