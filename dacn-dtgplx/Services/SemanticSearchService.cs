using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace dacn_dtgplx.Services
{
    public class SemanticSearchService
    {
        private readonly string _py;
        private readonly string _embedScript;
        private readonly string _searchScript;

        public SemanticSearchService(IWebHostEnvironment env)
        {
            _py = @"C:\Users\MSI\AppData\Local\Programs\Python\Python313\python.exe";
            _embedScript = Path.Combine(env.ContentRootPath, "PythonScripts", "embed_query.py");
            _searchScript = Path.Combine(env.ContentRootPath, "PythonScripts", "search_questions.py");
        }

        private async Task<List<float>> EmbedQuery(string keyword)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _py,
                Arguments = $"\"{_embedScript}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            string json = JsonSerializer.Serialize(new { query = keyword });
            await process.StandardInput.WriteAsync(json);
            process.StandardInput.Close();

            string result = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return JsonSerializer.Deserialize<List<float>>(result) ?? new();
        }

        public async Task<List<int>> SearchAsync(string keyword)
        {
            var emb = await EmbedQuery(keyword);
            if (emb.Count == 0) return new();

            var psi = new ProcessStartInfo
            {
                FileName = _py,
                Arguments = $"\"{_searchScript}\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            string json = JsonSerializer.Serialize(new { embedding = emb });
            await process.StandardInput.WriteAsync(json);
            process.StandardInput.Close();

            string result = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            return JsonSerializer.Deserialize<List<int>>(result) ?? new();
        }
    }

}
