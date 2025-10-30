using FeedbackAPI.Models;
using FeedbackAPI.Repositories;
using FeedbackAPI.Repositories.Interfaces;
using FeedbackAPI.Services.Interfaces;

namespace FeedbackAPI.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _FeedbackRepository;

        public FeedbackService(IFeedbackRepository FeedbackRepository)
        {
            _FeedbackRepository = FeedbackRepository;
        }

        public async Task<IEnumerable<Feedback>> GetAllFeedbacksAsync()
        {
            return await _FeedbackRepository.GetAllAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id)
        {
            return await _FeedbackRepository.GetByIdAsync(id);
        }

        public async Task CreateFeedbackAsync(Feedback Feedback)
        {
            await _FeedbackRepository.AddAsync(Feedback);
        }

        public async Task UpdateFeedbackAsync(Feedback Feedback)
        {
            await _FeedbackRepository.UpdateAsync(Feedback);
        }

        public async Task DeleteFeedbackAsync(int id)
        {
            await _FeedbackRepository.DeleteAsync(id);
        }
        public async Task<IEnumerable<Feedback>> GetFeedbacksByOrderIdAsync(int orderId)
        {
            return await _FeedbackRepository.GetByOrderIdAsync(orderId);
        }
    }
}
