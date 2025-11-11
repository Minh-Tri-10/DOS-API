using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrderAPI.Models;
using OrderAPI.Repositories;
using OrderAPI.Repositories.Interfaces;
using OrderAPI.Services;
using OrderAPI.Services.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using OrderAPI.DTOs;

namespace OrderAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("TriConnection")));




            builder.Services.AddHttpClient<ICategoryClient, CategoryClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7021/");
            });

            builder.Services.AddHttpClient<ICatalogProductClient, CatalogProductClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7021/");
            });

            builder.Services.AddHttpClient<IUserClient, UserClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7005/");
            });

            builder.Services.AddAutoMapper(typeof(OrderAPI.Profiles.OrderProfile).Assembly);
            builder.Services.AddScoped<IStatsService, StatsService>();
            builder.Services.AddScoped<IStatsRepository, StatsRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddHttpClient<IPaymentClient, PaymentClient>();

            var jwtSection = builder.Configuration.GetSection("Jwt");
            var signingKey = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException("JWT:Key configuration is missing for OrderAPI.");
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

            // --- OData setup ---
            builder.Services.AddControllers().AddOData(opt =>
                opt.AddRouteComponents("odata", GetEdmModel())
                   .Select()
                   .Filter()
                   .OrderBy()
                   .Expand()
                   .Count()
                   .SetMaxTop(100)
            );
            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        // --- OData EDM Model ---
        static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<OrderDto>("Orders"); // /odata/Orders
            builder.EntitySet<OrderItemDto>("OrderItems"); // optional nếu đại ca muốn expose item
            return builder.GetEdmModel();
        }
    }
}



