using MVCApplication.DTOs;
namespace MVCApplication.Services.Interfaces
{
    public interface IFeedbackService
    {
        Task<List<FeedbackResponseDTO>?> GetAllFeedbacksAsync();
        Task<FeedbackResponseDTO?> GetFeedbackByIdAsync(int id);
        Task<bool> CreateFeedbackAsync(FeedbackRequestDTO createDto);
        Task<bool> UpdateFeedbackAsync(int id, FeedbackUpdateDTO updateDto);
        Task<bool> DeleteFeedbackAsync(int id);
        Task<IEnumerable<FeedbackResponseDTO>> GetFeedbacksByOrderIdAsync(int orderId);

    }
}
