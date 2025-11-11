// AdminCategoryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;
using MVCApplication.DTOs; // Assuming models/DTOs are defined here
using Microsoft.Extensions.Logging;
namespace MVCApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<AdminCategoryController> _logger;
        public AdminCategoryController(ICategoryService categoryService, ILogger<AdminCategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger; // Inject logger
        }

        // GET: /AdminCategory/Index - Sử dụng service để gọi OData
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "", string orderBy = "CategoryId asc")
        {
            try
            {
                var (categories, totalCount) = await _categoryService.GetODataAsync(page, pageSize, search, orderBy);

                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                ViewBag.OrderBy = orderBy;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh mục OData");
                TempData["Error"] = "Không thể tải danh sách: " + ex.Message; // Hoặc hiển thị chi tiết hơn
                ModelState.AddModelError(string.Empty, "Không thể tải danh sách danh mục: " + ex.Message);
                return View(new List<CategoryDTO>());
            }
        }

        // GET: /AdminCategory/Create - Displays the create form
        public IActionResult Create()
        {
            return View();
        }

        // POST: /AdminCategory/Create - Handles creation of a new category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var newCategory = await _categoryService.AddAsync(dto);
                TempData["Success"] = "Category created successfully."; // Optional: Add success message
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                return View(dto);
            }
        }

        // GET: /AdminCategory/Edit/{id} - Displays the edit form for a specific category
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var dto = new UpdateCategoryDTO
            {
                CategoryName = category.CategoryName,
                Description = category.Description
            };

            return View(dto);
        }

        // POST: /AdminCategory/Edit/{id} - Handles updating a category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _categoryService.UpdateAsync(id, dto);
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                return View(dto);
            }
        }

        // GET: /AdminCategory/Details/{id} - Displays details of a specific category (added for completeness)
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        // POST: /AdminCategory/Delete/{id} - Handles deletion of a category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                // Ví dụ: “Không thể xóa vì danh mục có sản phẩm liên quan”
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

    }
}
