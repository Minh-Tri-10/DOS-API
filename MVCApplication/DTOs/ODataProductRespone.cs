using System.Text.Json.Serialization;

namespace MVCApplication.DTOs
{
    public class ODataProductRespone
    {
        public IEnumerable<ProductDTO> Value { get; set; } = Enumerable.Empty<ProductDTO>();
        [JsonPropertyName("@odata.count")]
        public int Count { get; set; }
    }
}
