namespace AccountAPI.Services.Email
{
    public sealed class EmailOptions
    {
        public string From { get; set; } = null!;
        public string FromName { get; set; } = "DrinkOrder Support";
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; } = 587;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool UseSsl { get; set; } = true;
    }
}
