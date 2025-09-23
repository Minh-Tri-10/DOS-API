using MVCApplication.Services;
using MVCApplication.Services.Interfaces;

namespace MVCApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();

            // ĐĂNG KÝ IAccountService + HttpClient (trỏ tới AccountAPI)
            builder.Services.AddHttpClient<IAccountService, AccountService>(c =>
            {
                c.BaseAddress = new Uri("https://localhost:7005/"); // ĐÚNG URL AccountAPI của bạn
            });
            builder.Services.AddHttpClient<IOrderService, OrdersService>(c =>
            {
                c.BaseAddress = new Uri("https://localhost:7269/");
            });
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Accounts}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
