namespace OrderAPI.Services.Interfaces
{
    public interface IPaymentClient
    {
        Task<string?> GetPaymentStatusByOrderIdAsync(int orderId);
    }
}
