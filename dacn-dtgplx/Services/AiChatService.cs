using dacn_dtgplx.Configs;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

public class AiChatService
{
    private readonly HttpClient _httpClient;

    public AiChatService()
    {
        _httpClient = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    public async Task<string> AskAsync(string userMessage)
    {
        var requestBody = new
        {
            model = "llama3",
            messages = new[]
            {
                new { role = "system", content = SystemPrompt.GPLX_VIETNAM },
                new { role = "user", content = userMessage }
            },
            stream = true
        };

        var json = JsonConvert.SerializeObject(requestBody);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "http://localhost:11434/api/chat"
        )
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead
        );

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        var sb = new StringBuilder();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            dynamic chunk = JsonConvert.DeserializeObject(line);

            if (chunk.message?.content != null)
                sb.Append((string)chunk.message.content);

            if (chunk.done == true)
                break;
        }

        return sb.ToString();
    }
}
