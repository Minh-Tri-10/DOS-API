using CategoriesAPI.DTOs;
using CategoriesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace CategoriesAPI.Controllers
{
    [ApiController]
    [Route("odata/Products")]  // Endpoint mới: api/odata/products
    [Authorize]
    public class ODataProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ODataProductController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery]
        public IQueryable<ProductDTO> Get()
        {
            return _service.GetAllQueryable();
        }
    }
}
