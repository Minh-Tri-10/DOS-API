using MVCApplication.Services;
using MVCApplication.Services.Interfaces;

namespace MVCApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== MVC + JSON =====
            builder.Services.AddControllersWithViews()
                .AddJsonOptions(opts =>
                {
                    // Cho phép deserialize JSON không phân biệt hoa/thường key
                    opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                });

            // Session + HttpContext
            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();

            // ===== HTTP CLIENTS (đổi BaseAddress theo cổng thật của bạn) =====
            // AccountAPI
            builder.Services.AddHttpClient<IAccountService, AccountService>(c =>
            {
                c.BaseAddress = new Uri("https://localhost:7005/"); // TODO: chỉnh cổng thật
            });

            // Orders (nếu bạn có FE OrdersService gọi OrderAPI phần đơn hàng)
            builder.Services.AddHttpClient<IOrderService, OrdersService>(c =>
            {
                c.BaseAddress = new Uri("https://localhost:7269/"); // TODO: chỉnh cổng thật
            });

            // Cart microservice (nếu có)
            builder.Services.AddHttpClient("CartAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7143/"); // TODO: chỉnh cổng thật
            });

            // Product microservice (nếu có)
            builder.Services.AddHttpClient("ProductAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7021/"); // TODO: chỉnh cổng thật
            });
            builder.Services.AddHttpClient<IPaymentService, PaymentService>("PaymentAPI", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7011/");
            });

            // HttpClient dùng chung để gọi OrderAPI (Stats endpoints)
            builder.Services.AddHttpClient("OrderApi", c =>
            {
                c.BaseAddress = new Uri("https://localhost:7269/"); // TODO: chỉnh cổng thật của OrderAPI
            });

            // ===== FE SERVICES (DI) =====
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICartService, CartService>();

            // FE StatsService: controller FE sẽ gọi service này, service gọi OrderAPI
            builder.Services.AddScoped<MVCApplication.Services.Interfaces.IStatsService,
                                       MVCApplication.Services.StatsService>();

            var app = builder.Build();

            // ===== PIPELINE =====
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();       // nếu dùng Session (login, cart, ...)
            app.UseAuthorization(); // nếu có [Authorize]

            // Conventional routing cho các trang MVC (ví dụ /Stats → StatsController.Index)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Accounts}/{action=Login}/{id?}");

            // Attribute routing cho các API có [Route(...)] (ví dụ /api/stats/*)
            app.MapControllers();

            app.Run();
        }
    }
}
