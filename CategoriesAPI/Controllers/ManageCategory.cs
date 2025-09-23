using AutoMapper;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;
using CategoriesAPI.Repositories.Interfaces;
using CategoriesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CategoriesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManageCategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly IMapper _mapper;
        public ManageCategoryController(ICategoryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newCategoryDto = await _service.AddAsync(dto);  // Lấy DTO với Id mới

            return CreatedAtAction(nameof(GetById), new { id = newCategoryDto.CategoryId }, newCategoryDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _service.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}