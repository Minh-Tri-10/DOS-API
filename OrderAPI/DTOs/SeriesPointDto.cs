namespace OrderAPI.DTOs
{
    public sealed class SeriesPointDto
    {
        public DateTime Bucket { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}
