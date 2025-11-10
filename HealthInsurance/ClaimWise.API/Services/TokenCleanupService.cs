using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClaimWise.Infrastructure.Data;
using ClaimWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClaimWise.API.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TokenCleanupService> _logger;

        public TokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<TokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ClaimWiseDbContext>();

                    var expiredTokens = await context.RefreshTokens
                        .Where(rt => rt.ExpiresAt < DateTime.UtcNow && !rt.IsRevoked)
                        .ToListAsync(stoppingToken);

                    if (expiredTokens.Any())
                    {
                        context.RefreshTokens.RemoveRange(expiredTokens);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"🧹 Cleaned up {expiredTokens.Count} expired refresh tokens at {DateTime.UtcNow}.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token cleanup.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Run every hour
            }
        }
    }
}
