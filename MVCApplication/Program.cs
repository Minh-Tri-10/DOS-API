using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using MVCApplication.Infrastructure;
using MVCApplication.Services;
using MVCApplication.Services.Interfaces;

namespace MVCApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<AccessTokenHandler>();

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Accounts/Login";
                    options.AccessDeniedPath = "/Accounts/Login";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                });

            builder.Services.AddAuthorization();

            builder.Services.AddHttpClient<IAccountService, AccountService>(c =>
            {
                c.BaseAddress = new Uri("https://localhost:7001/");
            }).AddHttpMessageHandler<AccessTokenHandler>();

            builder.Services.AddHttpClient<IOrderService, OrdersService>(c =>
            {
                c.BaseAddress = new Uri("https://localhost:7001/");
            }).AddHttpMessageHandler<AccessTokenHandler>();

            builder.Services.AddHttpClient("CartAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7001/");
            }).AddHttpMessageHandler<AccessTokenHandler>();

            builder.Services.AddHttpClient("ProductAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7001/");
            }).AddHttpMessageHandler<AccessTokenHandler>();

            builder.Services.AddHttpClient<IPaymentService, PaymentService>("PaymentAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7001/");
            }).AddHttpMessageHandler<AccessTokenHandler>();

            builder.Services.AddHttpClient("OrderApi", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7001/");
            }).AddHttpMessageHandler<AccessTokenHandler>();

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICartService, CartService>();

            builder.Services.AddScoped<MVCApplication.Services.Interfaces.IStatsService,
                                       MVCApplication.Services.StatsService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Accounts}/{action=Login}/{id?}");

            app.MapControllers();

            app.Run();
        }
    }
}
