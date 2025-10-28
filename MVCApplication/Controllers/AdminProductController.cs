using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

namespace MVCApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderService _orderService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminProductController(
            IProductService productService,
            ICategoryService categoryService,
            IOrderService orderService,
            IHttpClientFactory httpClientFactory)
        {
            _productService = productService;
            _categoryService = categoryService;
            _orderService = orderService;
            _httpClientFactory = httpClientFactory;
        }

        private int? CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }

        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Accounts");

            var products = await _productService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product == null ? NotFound() : View(product);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDTO dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(dto.CategoryId);
                return View(dto);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CategoriesAPI");
                using var formData = BuildMultipartPayload(dto, imageFile);

                var response = await client.PostAsync("api/Product", formData);
                response.EnsureSuccessStatusCode();

                TempData["Success"] = "San pham duoc tao thanh cong.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, "Khong the ket noi toi CategoriesAPI: " + ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Khong the tao san pham: " + ex.Message);
            }

            await PopulateCategoriesAsync(dto.CategoryId);
            return View(dto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var dto = new UpdateProductDTO
            {
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId
            };

            await PopulateCategoriesAsync(dto.CategoryId);
            ViewData["ProductId"] = product.ProductId;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductDTO dto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoriesAsync(dto.CategoryId);
                ViewData["ProductId"] = id;
                return View(dto);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CategoriesAPI");
                using var formData = BuildMultipartPayload(dto, imageFile);

                var response = await client.PutAsync($"api/Product/{id}", formData);
                response.EnsureSuccessStatusCode();

                TempData["Success"] = "San pham duoc cap nhat thanh cong.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, "Khong the ket noi toi CategoriesAPI: " + ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Khong the cap nhat san pham: " + ex.Message);
            }

            await PopulateCategoriesAsync(dto.CategoryId);
            ViewData["ProductId"] = id;
            return View(dto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product == null ? NotFound() : View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Kiểm tra xem Product có trong Order không
                var usage = await _orderService.CheckProductUsageAsync(id);  // Gọi API Order để kiểm tra
                if (usage.IsUsed)
                {
                    TempData["Error"] = $"Không thể xóa sản phẩm vì đang tồn tại trong {usage.OrderCount} đơn hàng.";
                    return RedirectToAction(nameof(Index));
                }

                // Nếu không có, tiến hành xóa
                await _productService.DeleteAsync(id);
                TempData["Success"] = "Sản phẩm được xóa thành công.";
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = "Không thể kết nối tới API: " + ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể xóa sản phẩm: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private MultipartFormDataContent BuildMultipartPayload(CreateProductDTO dto, IFormFile? imageFile)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(dto.ProductName ?? string.Empty), "ProductName" },
                { new StringContent(dto.Description ?? string.Empty), "Description" },
                { new StringContent(dto.Price.ToString()), "Price" },
                { new StringContent(dto.Stock?.ToString() ?? string.Empty), "Stock" },
                { new StringContent(dto.CategoryId?.ToString() ?? string.Empty), "CategoryId" }
            };

            if (imageFile != null)
            {
                var memoryStream = new MemoryStream();
                imageFile.CopyTo(memoryStream);
                memoryStream.Position = 0;
                formData.Add(new StreamContent(memoryStream), "ImageFile", imageFile.FileName);
            }

            return formData;
        }

        private MultipartFormDataContent BuildMultipartPayload(UpdateProductDTO dto, IFormFile? imageFile)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(dto.ProductName ?? string.Empty), "ProductName" },
                { new StringContent(dto.Description ?? string.Empty), "Description" },
                { new StringContent(dto.Price.ToString()), "Price" },
                { new StringContent(dto.Stock?.ToString() ?? string.Empty), "Stock" },
                { new StringContent(dto.CategoryId?.ToString() ?? string.Empty), "CategoryId" },
                { new StringContent(dto.ImageUrl ?? string.Empty), "ImageUrl" }
            };

            if (imageFile != null)
            {
                var memoryStream = new MemoryStream();
                imageFile.CopyTo(memoryStream);
                memoryStream.Position = 0;
                formData.Add(new StreamContent(memoryStream), "ImageFile", imageFile.FileName);
            }

            return formData;
        }

        private async Task PopulateCategoriesAsync(int? selectedCategoryId)
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", selectedCategoryId);
        }
    }
}

