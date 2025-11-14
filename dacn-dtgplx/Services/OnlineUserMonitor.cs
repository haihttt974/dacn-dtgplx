using dacn_dtgplx.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace dacn_dtgplx.Services
{
    public class OnlineUserMonitor : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OnlineUserMonitor> _logger;

        public OnlineUserMonitor(IServiceProvider services, ILogger<OnlineUserMonitor> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Lặp tới khi app dừng
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<DtGplxContext>();

                        var now = DateTime.UtcNow;
                        // Nếu hơn 15s không ping => coi như offline
                        var threshold = now.AddSeconds(-15);

                        var staleConnections = await context.WebsocketConnections
                            .Where(c => c.IsOnline &&
                                        (c.LastActivity == null || c.LastActivity < threshold))
                            .ToListAsync(stoppingToken);

                        if (staleConnections.Any())
                        {
                            foreach (var c in staleConnections)
                            {
                                c.IsOnline = false;
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi kiểm tra trạng thái online của user.");
                }

                // Chờ 5s rồi quét tiếp
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
