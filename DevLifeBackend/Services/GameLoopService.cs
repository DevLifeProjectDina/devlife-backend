// File: Services/GameLoopService.cs
using DevLifeBackend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DevLifeBackend.Services
{
    public class GameLoopService : BackgroundService
    {
        private readonly IHubContext<BugChaseHub> _hubContext;
        private readonly Random _random = new Random();

        public GameLoopService(IHubContext<BugChaseHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                if (_random.Next(1, 101) <= 80) // 80% шанс на препятствие
                {
                    // --- LOGIC UPDATE IS HERE ---
                    // Now we choose one of the three obstacle types randomly
                    string obstacleType;
                    int randomType = _random.Next(0, 3);
                    if (randomType == 0)
                    {
                        obstacleType = "Deadline";
                    }
                    else if (randomType == 1)
                    {
                        obstacleType = "Meeting"; // Added "Meeting"
                    }
                    else
                    {
                        obstacleType = "Bug";
                    }

                    var newObstacle = new
                    {
                        Id = Guid.NewGuid(),
                        PositionX = _random.Next(100, 1000),
                        PositionY = _random.Next(0, 2) == 0 ? 50 : 250,
                        Type = obstacleType,
                        Points = -25
                    };
                    await _hubContext.Clients.All.SendAsync("NewObstacleSpawned", newObstacle, stoppingToken);
                }
                else // 20% шанс на бонус
                {
                    var newPowerUp = new
                    {
                        Id = Guid.NewGuid(),
                        PositionX = _random.Next(100, 1000),
                        PositionY = _random.Next(0, 2) == 0 ? 50 : 250,
                        Type = _random.Next(0, 2) == 0 ? "Coffee" : "Weekend",
                        Points = 50
                    };
                    await _hubContext.Clients.All.SendAsync("NewPowerUpSpawned", newPowerUp, stoppingToken);
                }
            }
        }
    }
}