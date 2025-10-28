using Microsoft.Extensions.Logging;
using System.Text;
using CategoriesAPI.Mapping;
using CategoriesAPI.Models;
using CategoriesAPI.Repositories;
using CategoriesAPI.Repositories.Interfaces;
using CategoriesAPI.Services;
using CategoriesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CategoriesAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<CatalogDbContext>(options =>
<<<<<<< HEAD
                options.UseSqlServer(builder.Configuration.GetConnectionString("TriConnection")));
=======
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
>>>>>>> f56e4b37b49a48dd870c75b24ef39905e5e7485f

            builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddAutoMapper(typeof(CategoryProfile));
            builder.Services.AddAutoMapper(typeof(ProductProfile));

            var jwtSection = builder.Configuration.GetSection("Jwt");
            var signingKey = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException("JWT:Key configuration is missing for CategoriesAPI.");
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

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

