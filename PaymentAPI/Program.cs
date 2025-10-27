
using Microsoft.EntityFrameworkCore;
using PaymentAPI.Models;
using PaymentAPI.Profiles;
using PaymentAPI.Repositories;
using PaymentAPI.Repositories.Interfaces;
using PaymentAPI.Services;
using PaymentAPI.Services.Interfaces;

namespace PaymentAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Register DbContext
            builder.Services.AddDbContext<DrinkOrderDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("LocConnection")));
            // Add services to the container.
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddAutoMapper(typeof(PaymentProfile));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
