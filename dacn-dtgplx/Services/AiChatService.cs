//using dacn_dtgplx.Configs;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using System.Text;

//public class AiChatService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _apiKey;

//    public AiChatService(IConfiguration configuration)
//    {
//        _apiKey = configuration["GoogleAI:ApiKey"]
//            ?? throw new InvalidOperationException("Thiếu Google AI API Key");

//        _httpClient = new HttpClient
//        {
//            BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1/")
//        };
//    }

//    public async Task<string> AskAsync(string userMessage)
//    {
//        var prompt =
//            $"{SystemPrompt.GPLX_VIETNAM}\n\nNgười dùng: {userMessage}";

//        var requestBody = new
//        {
//            contents = new[]
//            {
//                new
//                {
//                    parts = new[]
//                    {
//                        new { text = prompt }
//                    }
//                }
//            }
//        };

//        var json = JsonConvert.SerializeObject(requestBody);

//        var response = await _httpClient.PostAsync(
//            $"models/gemini-2.5-flash:generateContent?key={_apiKey}",
//            new StringContent(json, Encoding.UTF8, "application/json")
//        );

//        var raw = await response.Content.ReadAsStringAsync();

//        if (!response.IsSuccessStatusCode)
//            throw new Exception(raw);

//        dynamic result = JsonConvert.DeserializeObject(raw);

//        return (string)result.candidates[0].content.parts[0].text;
//    }
//}
using dacn_dtgplx.Configs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

public class AiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AiChatService(IConfiguration configuration)
    {
        _apiKey = configuration["Perplexity:ApiKey"]
            ?? throw new InvalidOperationException("Thiếu Perplexity API Key");

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.perplexity.ai/")
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> AskAsync(string userMessage)
    {
        var requestBody = new
        {
            model = "sonar",  // hoặc "sonar-pro" nếu muốn kết quả tốt hơn
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = SystemPrompt.GPLX_VIETNAM
                },
                new
                {
                    role = "user",
                    content = userMessage
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);

        var response = await _httpClient.PostAsync(
            "chat/completions",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(raw);

        dynamic result = JsonConvert.DeserializeObject(raw);

        return (string)result.choices[0].message.content;
    }
}
