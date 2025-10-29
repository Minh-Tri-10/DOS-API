using System.Security.Claims;
using System.Text.Json.Serialization;

namespace MVCApplication.DTOs
{
    // Class helper cho OData response(có @odata.count)
    public class ODataCateResponse
    {
        public IEnumerable<CategoryDTO> Value { get; set; } = Enumerable.Empty<CategoryDTO>();
        [JsonPropertyName("@odata.count")]
        public int Count { get; set; }
    }
}
