using AccountAPI.Repositories.Interfaces;
using AccountAPI.Repositories;
using AccountAPI.Services.Interfaces;
using AccountAPI.Services;
using AccountAPI.Models;
using AccountAPI.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace AccountAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMemoryCache();
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(typeof(AccountAPI.Mapping.MappingProfile));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DrinkOrderContext>(options =>

                options.UseSqlServer(builder.Configuration.GetConnectionString("HuyConnection")));

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

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
