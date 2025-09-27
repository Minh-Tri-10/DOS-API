using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;
using System.Security.Claims;

    namespace MVCApplication.Controllers
    {
        public class AdminProductController : Controller
        {
            private readonly IProductService _productService;
            private readonly ICategoryService _categoryService;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminProductController(IProductService productService, ICategoryService categoryService, IHttpClientFactory httpClientFactory)
        {
            _productService = productService;
            _categoryService = categoryService;
            _httpClientFactory = httpClientFactory;
        }

        // Property helper để lấy CurrentUserId từ Claims (hoặc thay bằng Session.GetInt32("UserId") nếu dùng session)
        private int? CurrentUserId => HttpContext.Session.GetInt32("UserId");

        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null) return RedirectToAction("Login", "Accounts");
            var products = await _productService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);
            var debug = string.Join(", ", categories.Select(c => $"{c.CategoryId}:{c.CategoryName}"));
            Console.WriteLine(debug);
            return View(products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int id)
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }

            // GET: Product/Create
            public async Task<IActionResult> Create()
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
                return View();
            }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDTO dto, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(dto.ProductName), "ProductName");
                    formData.Add(new StringContent(dto.Description ?? ""), "Description");
                    formData.Add(new StringContent(dto.Price.ToString()), "Price");
                    formData.Add(new StringContent(dto.Stock?.ToString() ?? ""), "Stock");
                    formData.Add(new StringContent(dto.CategoryId?.ToString() ?? ""), "CategoryId");
                    if (imageFile != null)
                    {
                        var memoryStream = new MemoryStream();
                        await imageFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // Reset vị trí đọc
                        formData.Add(new StreamContent(memoryStream), "ImageFile", imageFile.FileName);
                    }
                    Console.WriteLine($"ImageFile từ form: {(imageFile != null ? "Có file, tên: " + imageFile.FileName : "Null")}");
                    var response = await client.PostAsync("https://localhost:7021/api/Product", formData);
                    response.EnsureSuccessStatusCode();               
                    TempData["Success"] = "Sản phẩm được tạo thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError("", "Không thể kết nối đến CategoryAPI: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể tạo sản phẩm: " + ex.Message);
                }
            }

            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", dto.CategoryId);
            return View(dto);
        }


        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int id)
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var updateDto = new UpdateProductDTO
                {
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId
                };

                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", updateDto.CategoryId);

                ViewData["ProductId"] = product.ProductId;
                return View(updateDto);
            }


        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductDTO dto, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(dto.ProductName), "ProductName");
                    formData.Add(new StringContent(dto.Description ?? ""), "Description");
                    formData.Add(new StringContent(dto.Price.ToString()), "Price");
                    formData.Add(new StringContent(dto.Stock?.ToString() ?? ""), "Stock");
                    formData.Add(new StringContent(dto.CategoryId?.ToString() ?? ""), "CategoryId");
                    formData.Add(new StringContent(dto.ImageUrl ?? ""), "ImageUrl");
                    if (imageFile != null)
                    {
                        var memoryStream = new MemoryStream();
                        await imageFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // Reset vị trí đọc
                        formData.Add(new StreamContent(memoryStream), "ImageFile", imageFile.FileName);
                    }

                    var response = await client.PutAsync($"https://localhost:7021/api/Product/{id}", formData);
                    response.EnsureSuccessStatusCode();
                    TempData["Success"] = "Sản phẩm được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (HttpRequestException ex)
                {
                    ModelState.AddModelError("", "Không thể kết nối đến CategoryAPI: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Không thể cập nhật sản phẩm: " + ex.Message);
                }
            }

            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", dto.CategoryId);
            ViewData["ProductId"] = id;
            return View(dto);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int id)
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }

            // POST: Product/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                try
                {
                    await _productService.DeleteAsync(id);
                    TempData["Success"] = "Sản phẩm được xóa thành công.";
                }
                catch
                {
                    TempData["Error"] = "Không thể xóa sản phẩm.";
                }
                return RedirectToAction(nameof(Index));
            }
        }
    }
