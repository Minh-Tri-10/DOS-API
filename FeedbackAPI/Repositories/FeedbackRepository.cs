using Microsoft.EntityFrameworkCore;
using System;
using FeedbackAPI.Models;
using FeedbackAPI.Repositories.Interfaces;

namespace FeedbackAPI.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly DosfeedbackDbContext _context;

        public FeedbackRepository(DosfeedbackDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            return await _context.Feedbacks.ToListAsync();
        }

        public async Task<Feedback?> GetByIdAsync(int id)
        {
            return await _context.Feedbacks.FindAsync(id);
        }

        public async Task AddAsync(Feedback Feedback)
        {
            _context.Feedbacks.Add(Feedback);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Feedback Feedback)
        {
            _context.Feedbacks.Update(Feedback);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var Feedback = await _context.Feedbacks.FindAsync(id);
            if (Feedback != null)
            {
                _context.Feedbacks.Remove(Feedback);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Feedback>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Feedbacks
                .Where(f => f.OrderId == orderId) // Giả định mô hình Feedback có thuộc tính OrderId (string)
                .ToListAsync();
        }
    }
}
