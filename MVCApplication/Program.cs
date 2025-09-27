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
            builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

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
            builder.Services.AddScoped<IProductService, ProductService>();
            //builder.Services.AddHttpClient<IAccountService, AccountService>();
            builder.Services.AddHttpClient("CartAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7143/");
            });

            builder.Services.AddHttpClient("ProductAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7014/");
            });
            builder.Services.AddHttpClient<IPaymentService, PaymentService>("PaymentAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7011/");
            });

            // Đăng ký ICartService dùng factory
            builder.Services.AddScoped<ICartService, CartService>();

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
