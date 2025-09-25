
using Microsoft.EntityFrameworkCore;
using OrderAPI.Repositories;
using OrderAPI.Repositories.Interfaces;
using OrderAPI.Services;
using OrderAPI.Services.Interfaces;
using OrderAPI.Models;
namespace OrderAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(Program));

            builder.Services.AddDbContext<DrinkOrderDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("TriConnection")));


            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
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
