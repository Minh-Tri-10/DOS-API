using System.Collections.Generic;

namespace MVCApplication.DTOs
{
    public class OdataFeedbackResponseDTO<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Value { get; set; } = Enumerable.Empty<T>();
    }
}
