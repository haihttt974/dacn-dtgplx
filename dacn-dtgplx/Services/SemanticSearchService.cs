using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace dacn_dtgplx.Services
{
    public class SemanticSearchService
    {
        private readonly string _pythonExePath;
        private readonly string _scriptPath;
        private readonly string _apiKey;

        public SemanticSearchService(IConfiguration config, IWebHostEnvironment env)
        {
            // ĐƯỜNG DẪN PYTHON CỦA BẠN
            _pythonExePath = @"C:\Users\MSI\AppData\Local\Programs\Python\Python313\python.exe";

            // Đường dẫn tới script search_questions.py trong project
            _scriptPath = Path.Combine(env.ContentRootPath, "PythonScripts", "search_questions.py");

            _apiKey = config["OpenAI:ApiKey"] ?? "";
        }

        public async Task<List<int>> SearchAsync(string keyword, int take = 20)
        {
            var resultIds = new List<int>();

            if (string.IsNullOrWhiteSpace(keyword))
                return resultIds;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _pythonExePath,
                    Arguments = $"\"{_scriptPath}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                var payload = new
                {
                    query = keyword,
                    apiKey = _apiKey
                };

                string jsonInput = JsonSerializer.Serialize(payload);

                await process.StandardInput.WriteAsync(jsonInput);
                process.StandardInput.Close();

                string stdout = await process.StandardOutput.ReadToEndAsync();
                //string stderr = await process.StandardError.ReadToEndAsync();
                string stderr = await process.StandardError.ReadToEndAsync();
                Console.WriteLine("=== PYTHON STDERR ===");
                Console.WriteLine(stderr);
                Console.WriteLine("=== PYTHON STDOUT RAW ===");
                Console.WriteLine(stdout);

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Console.WriteLine("PYTHON ERROR: " + stderr);
                }

                if (string.IsNullOrWhiteSpace(stdout) || stdout.Trim() == "[]")
                    return resultIds;

                try
                {
                    var ids = JsonSerializer.Deserialize<List<int>>(stdout);
                    if (ids != null)
                    {
                        if (take > 0)
                            ids = ids.GetRange(0, Math.Min(take, ids.Count));
                        resultIds = ids;
                    }
                }
                catch (Exception parseEx)
                {
                    Console.WriteLine("Parse python JSON error: " + parseEx.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SemanticSearchService error: " + ex.Message);
            }

            return resultIds;
        }
    }
}
