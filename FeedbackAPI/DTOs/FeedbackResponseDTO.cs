using System;

namespace FeedbackAPI.DTOs
{
    public class FeedbackResponseDTO
    {
        public int FeedbackId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime FeedbackDate { get; set; }
    }
}