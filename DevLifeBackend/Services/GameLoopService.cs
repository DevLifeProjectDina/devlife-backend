using DevLifeBackend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace DevLifeBackend.Services
{
    public class GameLoopService : BackgroundService
    {
        private readonly IHubContext<BugChaseHub> _hubContext;
        private readonly ILogger<GameLoopService> _logger;
        private readonly Random _random = new();

        public GameLoopService(IHubContext<BugChaseHub> hubContext, ILogger<GameLoopService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GameLoopService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                if (_random.Next(1, 101) <= 80)
                {
                    string obstacleType;
                    int randomType = _random.Next(0, 3);
                    if (randomType == 0)
                    {
                        obstacleType = "Deadline";
                    }
                    else if (randomType == 1)
                    {
                        obstacleType = "Meeting";
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

                    _logger.LogInformation("Spawning new obstacle: {ObstacleType} at X:{PositionX}", newObstacle.Type, newObstacle.PositionX);
                    await _hubContext.Clients.All.SendAsync("NewObstacleSpawned", newObstacle, stoppingToken);
                }
                else
                {
                    var newPowerUp = new
                    {
                        Id = Guid.NewGuid(),
                        PositionX = _random.Next(100, 1000),
                        PositionY = _random.Next(0, 2) == 0 ? 50 : 250,
                        Type = _random.Next(0, 2) == 0 ? "Coffee" : "Weekend",
                        Points = 50
                    };

                    _logger.LogInformation("Spawning new power-up: {PowerUpType} at X:{PositionX}", newPowerUp.Type, newPowerUp.PositionX);
                    await _hubContext.Clients.All.SendAsync("NewPowerUpSpawned", newPowerUp, stoppingToken);
                }
            }

            _logger.LogInformation("GameLoopService is stopping.");
        }
    }
}