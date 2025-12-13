using Microsoft.AspNetCore.Mvc;

namespace dacn_dtgplx.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly AiChatService _aiChatService;

        // Constructor
        public ChatbotController()
        {
            _aiChatService = new AiChatService();
        }

        // GET: /Chatbot
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Chatbot/SendMessage
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    error = "Câu hỏi không hợp lệ."
                });
            }

            try
            {
                var reply = await _aiChatService.AskAsync(request.Message);

                return Json(new
                {
                    reply = reply
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new
                {
                    error = "Không kết nối được AI. Vui lòng kiểm tra Ollama."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Đã xảy ra lỗi trong quá trình xử lý.",
                    detail = ex.Message
                });
            }
        }
    }

    // DTO nhận dữ liệu từ frontend
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
