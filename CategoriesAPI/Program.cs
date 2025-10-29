using CategoriesAPI.DTOs;
using CategoriesAPI.Mapping;
using CategoriesAPI.Models;
using CategoriesAPI.Repositories;
using CategoriesAPI.Repositories.Interfaces;
using CategoriesAPI.Services;
using CategoriesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Text;

namespace CategoriesAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddAutoMapper(typeof(CategoryProfile));
            builder.Services.AddAutoMapper(typeof(ProductProfile));

            builder.Services.AddControllers().AddOData(opt =>
                opt.AddRouteComponents("odata", GetEdmModel())
                   .Select()
                   .Filter()
                   .OrderBy()
                   .Expand()
                   .Count()
                   .SetMaxTop(100)
            );

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

        static IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<CategoryDTO>("Categories"); // Plural, phải khớp với route "odata/Categories"
            odataBuilder.EntitySet<ProductDTO>("Products");
            return odataBuilder.GetEdmModel();
        }
    }
}

