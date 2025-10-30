using AutoMapper;
using FeedbackAPI.DTOs;
using FeedbackAPI.Models;
using FeedbackAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FeedbackAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IMapper _mapper; // dùng DTO

        public FeedbacksController(IFeedbackService feedbackService, IMapper mapper)
        {
            _feedbackService = feedbackService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeedbackResponseDTO>>> GetFeedbacks()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            return Ok(_mapper.Map<IEnumerable<FeedbackResponseDTO>>(feedbacks));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FeedbackResponseDTO>> GetFeedback(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);

            if (feedback == null)
            {
                return NotFound();
            }

            return _mapper.Map<FeedbackResponseDTO>(feedback);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFeedback(int id, FeedbackUpdateDTO feedbackUpdateDto)
        {
            var existingFeedback = await _feedbackService.GetFeedbackByIdAsync(id);

            if (existingFeedback == null)
            {
                return NotFound();
            }

            // Ánh xạ DTO lên bản ghi gốc. 
            // Chỉ Rating và Comment được cập nhật, các trường khác (OrderId, FeedbackDate) giữ nguyên.
            _mapper.Map(feedbackUpdateDto, existingFeedback);

            try
            {
                await _feedbackService.UpdateFeedbackAsync(existingFeedback);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _feedbackService.GetFeedbackByIdAsync(id) == null)
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<FeedbackResponseDTO>> PostFeedback(FeedbackRequestDTO feedbackRequestDto)
        {
            var feedback = _mapper.Map<Feedback>(feedbackRequestDto);

            await _feedbackService.CreateFeedbackAsync(feedback);

            var feedbackResponseDto = _mapper.Map<FeedbackResponseDTO>(feedback);

            return CreatedAtAction(nameof(GetFeedback), new { id = feedback.FeedbackId }, feedbackResponseDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            await _feedbackService.DeleteFeedbackAsync(id);

            return NoContent();
        }

        [HttpGet("odata")]
        [EnableQuery]
        public async Task<IActionResult> GetFeedbacksOData()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            var mappedFeedbacks = _mapper.Map<IEnumerable<FeedbackResponseDTO>>(feedbacks);

            return Ok(mappedFeedbacks);
        }

        [HttpGet("byorder/{orderId}")]
        public async Task<ActionResult<IEnumerable<FeedbackResponseDTO>>> GetFeedbacksByOrderId(int orderId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByOrderIdAsync(orderId);

            if (feedbacks == null || !feedbacks.Any())
            {
                // Trả về 404 nếu không tìm thấy phản hồi nào cho OrderId này
                return NotFound($"Không tìm thấy phản hồi cho OrderId: {orderId}");
            }

            // Ánh xạ sang DTO và trả về 200 OK
            return Ok(_mapper.Map<IEnumerable<FeedbackResponseDTO>>(feedbacks));
        }
    }
}