using Microsoft.AspNetCore.Mvc;
using MVCApplication.Services.Interfaces;
using MVCApplication.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVCApplication.Controllers
{
    public class FeedbacksController : Controller
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbacksController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        // GET: /Feedbacks/Index
        public async Task<IActionResult> Index()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacksAsync();
            // Trả về danh sách, nếu null thì trả về list rỗng
            return View(feedbacks ?? new List<FeedbackResponseDTO>());
        }

        // GET: /Feedbacks/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // GET: /Feedbacks/Create
        public IActionResult Create()
        {
            var model = new FeedbackRequestDTO();

            // 🌟 Lấy giá trị OrderId từ TempData
            if (TempData.ContainsKey("PreFillOrderId"))
            {
                // Gán giá trị vào ViewData để View sử dụng
                ViewData["OrderId"] = TempData["PreFillOrderId"];
            }

            return View(model);
        }

        // POST: /Feedbacks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FeedbackRequestDTO createDto)
        {
            if (ModelState.IsValid)
            {
                var success = await _feedbackService.CreateFeedbackAsync(createDto);
                if (success)
                {
                    // 🌟 Gửi thông báo thành công qua TempData
                    TempData["StatusMessage"] = $"Đánh giá cho Đơn hàng #{createDto.OrderId} đã được tạo thành công!";
                    return RedirectToAction("Index", "Orders");
                }
                // Thêm lỗi nếu API trả về lỗi
                ModelState.AddModelError("", "Lỗi khi gọi API tạo Feedback. Vui lòng thử lại.");
            }
            return View(createDto);
        }

        // GET: /Feedbacks/Edit/5 (Lấy dữ liệu cũ để hiển thị)
        public async Task<IActionResult> Edit(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            // Map ResponseDTO (dữ liệu đầy đủ) sang UpdateDTO (dữ liệu form)
            var updateDto = new FeedbackUpdateDTO
            {
                Rating = feedback.Rating,
                Comment = feedback.Comment
            };

            // Truyền ID vào ViewData để dùng trong POST Edit
            ViewData["FeedbackId"] = id;

            // Bổ sung: Truyền OrderId vào ViewData để hiển thị trên View
            ViewData["OrderId"] = feedback.OrderId;

            return View(updateDto);
        }

        // POST: /Feedbacks/Edit/5 (Gửi dữ liệu cập nhật)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FeedbackUpdateDTO updateDto)
        {
            if (id <= 0) return NotFound();

            if (ModelState.IsValid)
            {
                var success = await _feedbackService.UpdateFeedbackAsync(id, updateDto);
                if (success)
                {
                    // 🌟 Gửi thông báo thành công qua TempData
                    TempData["StatusMessage"] = $"Đánh giá #{id} đã được cập nhật thành công!";
                    return RedirectToAction("Index", "Orders");
                }
                ModelState.AddModelError("", "Lỗi khi gọi API cập nhật Feedback. Vui lòng kiểm tra ID.");
            }
            ViewData["FeedbackId"] = id;
            return View(updateDto);
        }

        // POST: /Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _feedbackService.DeleteFeedbackAsync(id);
            if (success)
            {
                // 🌟 Gửi thông báo thành công qua TempData
                TempData["StatusMessage"] = $"Đánh giá #{id} đã được xóa thành công!";
                return RedirectToAction("Index", "Orders");
            }

            // Nếu xóa lỗi, chuyển hướng và thông báo lỗi.
            TempData["StatusMessage"] = $"Lỗi: Không thể xóa Đánh giá #{id}.";
            return RedirectToAction("Index", "Orders");
        }

        // GET: /Feedbacks/UpsertByOrder?orderId=123 check xem đi đến trang feedback nào dựa trên OrderId
        public async Task<IActionResult> UpsertByOrder(int orderId)
        {
            var feedbacks = await _feedbackService.GetFeedbacksByOrderIdAsync(orderId);
            var existingFeedback = feedbacks.FirstOrDefault();

            if (existingFeedback != null)
            {
                return RedirectToAction(nameof(Edit), new { id = existingFeedback.FeedbackId });
            }
            else
            {
                TempData["PreFillOrderId"] = orderId;

                return RedirectToAction(nameof(Create));
            }
        }
    }
}
