using CategoriesAPI.DTOs;
using CategoriesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace CategoriesAPI.Controllers
{
    [ApiController]
    [Route("odata/Categories")]  // Endpoint mới: api/odata/categories
    [Authorize]  // Giữ quyền Admin
    public class ODataCategoryController : ControllerBase
    {
        private readonly ICategoryService _service;  // Giả sử service của bạn là ICategoryService

        public ODataCategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]  // Kích hoạt OData query
        public IQueryable<CategoryDTO> Get()
        {
            // Trả về IQueryable để OData xử lý filter, orderby, top, skip
            return _service.GetAllQueryable();  // Cần thêm method này trong service
        }
    }
}
