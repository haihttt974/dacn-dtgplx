using Microsoft.AspNetCore.Mvc;

namespace dacn_dtgplx.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly AiChatService _aiChatService;

        public ChatbotController(AiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "Câu hỏi không hợp lệ." });
            }

            try
            {
                var reply = await _aiChatService.AskAsync(request.Message);
                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Gemini API error",
                    detail = ex.Message
                });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
