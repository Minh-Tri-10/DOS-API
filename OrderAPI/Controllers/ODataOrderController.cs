using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using OrderAPI.DTOs;
using OrderAPI.Services.Interfaces;

namespace OrderAPI.Controllers
{
    [ApiController]
    [Route("odata/Orders")]
    //[Authorize(Roles = "Admin")]
    public class ODataOrderController : ODataController
    {
        private readonly IOrderService _service;

        public ODataOrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        [EnableQuery] // Cho phép dùng $filter, $select, $orderby, $top, $skip
        public IQueryable<OrderDto> Get()
        {
            return _service.GetAllQueryable();
        }
    }
}
