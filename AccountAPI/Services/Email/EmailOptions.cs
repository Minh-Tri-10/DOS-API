namespace AccountAPI.Services.Email
{
    public sealed class EmailOptions
    {
        public string ServiceId { get; set; } = null!;
        public string TemplateId { get; set; } = null!;
        public string PublicKey { get; set; } = null!;
        public string? AccessToken { get; set; }
            = null;
        public string FromName { get; set; } = "DrinkOrder Support";
        public string Origin { get; set; } = "https://localhost";
    }
}

