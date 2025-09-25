// AdminCategoryController.cs
using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;
using MVCApplication.DTOs; // Assuming models/DTOs are defined here

namespace MVCApplication.Controllers
{
    public class AdminCategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: /AdminCategory/Index - Displays a list of all categories
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
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

        // GET: /AdminCategory/Delete/{id} - Displays the delete confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /AdminCategory/Delete/{id} - Handles deletion of a category
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                TempData["Success"] = "Category deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}