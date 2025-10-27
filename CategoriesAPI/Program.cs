
using CategoriesAPI.Mapping;
using CategoriesAPI.Models;
using CategoriesAPI.Repositories;
using CategoriesAPI.Repositories.Interfaces;
using CategoriesAPI.Services;
using CategoriesAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProductAPI.Mapping;
using ProductAPI.Repositories;
using ProductAPI.Services;

namespace CategoriesAPI
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
            builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
            builder.Services.AddDbContext<DrinkOrderDbContext>(options =>

    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repositories và Services
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Configuration.AddUserSecrets<Program>();
            // AutoMapper
            builder.Services.AddAutoMapper(typeof(CategoryProfile)); // MappingProfile như trước
            // Đăng ký AutoMapper
            builder.Services.AddAutoMapper(typeof(ProductProfile));

            // Đăng ký DI cho Repository và Service
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();
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
