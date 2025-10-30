using FeedbackAPI.Models;

namespace FeedbackAPI.Repositories.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback?> GetByIdAsync(int id);
        Task AddAsync(Feedback Feedback);
        Task UpdateAsync(Feedback Feedback);
        Task DeleteAsync(int id);
        Task<IEnumerable<Feedback>> GetByOrderIdAsync(int orderId);
    }
}
