using AccountAPI.Repositories.Interfaces;
using AccountAPI.Repositories;
using AccountAPI.Services.Interfaces;
using AccountAPI.Services;
using AccountAPI.Models;
using AccountAPI.Services.Email;  // <--- NEW
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
                options.UseSqlServer(builder.Configuration.GetConnectionString("WeiConnection")));

            // Email DI (NEW)
            builder.Services.AddSingleton(sp =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>().GetSection("Email");
                return new EmailOptions
                {
                    From = cfg["From"]!,
                    FromName = cfg["FromName"] ?? "DrinkOrder Support (DEV)",
                    SmtpHost = cfg["SmtpHost"]!,
                    SmtpPort = int.Parse(cfg["SmtpPort"] ?? "587"),
                    Username = cfg["Username"] ?? "",
                    Password = cfg["Password"] ?? "",
                    UseSsl = bool.Parse(cfg["UseSsl"] ?? "true")
                };
            });
            builder.Services.AddSingleton<IEmailSender, EmailSender>();

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
