using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCApplication.DTOs;
using MVCApplication.Services.Interfaces;

    namespace MVCApplication.Controllers
    {
        public class AdminProductController : Controller
        {
            private readonly IProductService _productService;
            private readonly ICategoryService _categoryService;

            public AdminProductController(IProductService productService, ICategoryService categoryService)
            {
                _productService = productService;
                _categoryService = categoryService;
            }

            public async Task<IActionResult> Index()
            {
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
            public async Task<IActionResult> Create(CreateProductDTO dto)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        await _productService.CreateAsync(dto);
                        TempData["Success"] = "Sản phẩm được tạo thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Không thể tạo sản phẩm.");
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
            public async Task<IActionResult> Edit(int id, UpdateProductDTO dto)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        await _productService.UpdateAsync(id, dto);
                        TempData["Success"] = "Sản phẩm được cập nhật thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Không thể cập nhật sản phẩm.");
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
