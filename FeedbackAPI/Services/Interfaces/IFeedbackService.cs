using FeedbackAPI.Models;

namespace FeedbackAPI.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<IEnumerable<Feedback>> GetAllFeedbacksAsync();
        Task<Feedback?> GetFeedbackByIdAsync(int id);
        Task CreateFeedbackAsync(Feedback Feedback);
        Task UpdateFeedbackAsync(Feedback Feedback);
        Task DeleteFeedbackAsync(int id);

        Task<IEnumerable<Feedback>> GetFeedbacksByOrderIdAsync(int orderId);
    }
}
