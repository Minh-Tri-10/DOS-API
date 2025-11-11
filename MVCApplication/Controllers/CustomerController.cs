using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{

    public class CustomerController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        public CustomerController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        private int? CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(userIdClaim, out var id) ? id : null;
            }
        }

        public async Task<IActionResult> Index(int? categoryId = null, int page = 1, int pageSize = 100, string search = "", string orderBy = "ProductName asc")
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Accounts");
            // Fetch products với filter categoryId và search
            var (products, totalCount) = await _productService.GetODataAsync(page, pageSize, search, orderBy, categoryId);

            // Fetch categories (giữ nguyên)
            var (categories, catTotalCount) = await _categoryService.GetODataAsync(
                page: 1,
                pageSize: 100, // Lấy đủ danh mục
                search: "",
                orderBy: "CategoryName asc"
            );

            // Set ViewBag để đồng bộ trạng thái
            ViewBag.Categories = categories;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.Search = search; // Thêm cái này để giữ giá trị input search sau redirect
            ViewBag.CurrentUserId = CurrentUserId ?? 0; // Thêm cái này để script JS sử dụng

            return View(products);
        }
    }
}
