namespace OrderAPI.Services.Interfaces
{
    public interface IUserClient
    {
        Task<string?> GetFullNameByIdAsync(int userId);
    }
}
