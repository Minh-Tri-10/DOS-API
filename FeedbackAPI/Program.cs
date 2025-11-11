using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Repositories;
using FeedbackAPI.Repositories.Interfaces;
using FeedbackAPI.Services;
using FeedbackAPI.Services.Interfaces;
// 1. THÊM USING CHO ODATA
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Text;

namespace FeedbackAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Repository DI
            builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            // Service DI
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            // Profile AutoMapper
            builder.Services.AddAutoMapper(typeof(FeedbackAPI.Profiles.FeedbackProfile));

            // Odata Configuration
            var odatabuilder = new ODataConventionModelBuilder();

            // SỬA LỖI CS1061: Chỉ định khóa chính cho kiểu Entity
            odatabuilder.EntityType<FeedbackResponseDTO>().HasKey(f => f.FeedbackId);

            // Định nghĩa EntitySet sử dụng kiểu đã cấu hình
            odatabuilder.EntitySet<FeedbackResponseDTO>("Feedbacks");

            // Add services to the container.
            builder.Services.AddControllers().AddOData(options =>
            {
                options
                    .Select()
                    .Filter()
                    .OrderBy()
                    .SetMaxTop(100)
                    .Count()
                    .Expand()
                    .AddRouteComponents("odata", odatabuilder.GetEdmModel());
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<DosfeedbackDbContext>(option =>
            option.UseSqlServer(builder.Configuration.GetConnectionString("LocConnection")));


            var jwtSection = builder.Configuration.GetSection("Jwt");
            var signingKey = jwtSection["Key"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                throw new InvalidOperationException("JWT:Key configuration is missing for FeedbackAPI.");
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
