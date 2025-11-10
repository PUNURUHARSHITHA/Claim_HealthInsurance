using System.Text;

using ClaimWise.Application.DTOs;

using ClaimWise.Infrastructure.Data;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

namespace ClaimWise.API.Controllers

{

    [ApiController]

    [Route("api/[controller]")]

    [Authorize(Roles = "Admin")]

    public class AdminController : ControllerBase

    {

        private readonly ClaimWiseDbContext _dbContext;

        public AdminController(ClaimWiseDbContext dbContext)

        {

            _dbContext = dbContext;

        }

        // ✅ GET: /api/Admin/login-audit

        [HttpGet("login-audit")]

        public async Task<IActionResult> GetLoginAuditLogs(

            [FromQuery] string? username,

            [FromQuery] string? ip,

            [FromQuery] DateTime? from,

            [FromQuery] DateTime? to)

        {

            var query = _dbContext.LoginAuditLogs

                .Include(l => l.User)

                .AsQueryable();

            if (!string.IsNullOrEmpty(username))

                query = query.Where(l => l.Username == username);

            if (!string.IsNullOrEmpty(ip))

                query = query.Where(l => l.IPAddress != null && l.IPAddress == ip);

            if (from.HasValue)

                query = query.Where(l => l.Timestamp >= from.Value);

            if (to.HasValue)

                query = query.Where(l => l.Timestamp <= to.Value);

            var logs = await query

                .OrderByDescending(l => l.Timestamp)

                .Take(100)

                .Select(l => new LoginAuditLogDto

                {

                    LogID = l.LogID,

                    Username = l.Username,

                    IsSuccessful = l.IsSuccessful,

                    IPAddress = l.IPAddress ?? "N/A",   // ✅ Safe fallback

                    UserAgent = l.UserAgent ?? "N/A",   // ✅ Safe fallback

                    Timestamp = l.Timestamp,

                    Role = l.User != null ? l.User.Role : null

                })

                .ToListAsync();

            return Ok(logs);

        }

        // 📤 GET: /api/Admin/login-audit/export

        [HttpGet("login-audit/export")]

        public async Task<IActionResult> ExportLoginAuditLogsToCsv()

        {

            var logs = await _dbContext.LoginAuditLogs

                .Include(l => l.User)

                .OrderByDescending(l => l.Timestamp)

                .Take(100)

                .ToListAsync();

            var csv = new StringBuilder();

            csv.AppendLine("LogID,Username,IsSuccessful,IPAddress,UserAgent,Timestamp,Role");

            foreach (var log in logs)

            {

                var role = log.User?.Role ?? "";

                var ip = log.IPAddress ?? "N/A";

                var agent = log.UserAgent ?? "N/A";

                csv.AppendLine($"\"{log.LogID}\",\"{log.Username}\",\"{log.IsSuccessful}\",\"{ip}\",\"{agent}\",\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{role}\"");

            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", "LoginAuditLogs.csv");

        }

        // 📊 GET: /api/Admin/login-audit/stats

        [HttpGet("login-audit/stats")]

        [ProducesResponseType(StatusCodes.Status200OK)]

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetLoginStats()

        {

            var grouped = await _dbContext.LoginAuditLogs

                .GroupBy(l => l.Timestamp.Date)

                .Select(g => new

                {

                    Date = g.Key,

                    Total = g.Count(),

                    Success = g.Count(l => l.IsSuccessful),

                    Failure = g.Count(l => !l.IsSuccessful)

                })

                .OrderByDescending(g => g.Date)

                .Take(30)

                .ToListAsync();

            var result = grouped.Select(g => new

            {

                Date = g.Date.ToString("yyyy-MM-dd"),

                g.Total,

                g.Success,

                g.Failure

            });

            return Ok(result);

        }

        // 🧑‍💼 GET: /api/Admin/active-sessions/{userId}

        [HttpGet("active-sessions/{userId}")]

        [ProducesResponseType(StatusCodes.Status200OK)]

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetSessions(int userId)

        {

            var sessions = await _dbContext.RefreshTokens

                .Where(rt => rt.UserID == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)

                .OrderByDescending(rt => rt.CreatedAt)

                .Select(rt => new

                {

                    rt.Token,

                    rt.CreatedAt,

                    rt.ExpiresAt,

                    rt.IsRevoked

                })

                .ToListAsync();

            return Ok(sessions);

        }

        // 🔐 POST: /api/Admin/revoke-all/{userId}

        [HttpPost("revoke-all/{userId}")]

        [ProducesResponseType(StatusCodes.Status200OK)]

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> RevokeAllTokens(int userId)

        {

            var tokens = await _dbContext.RefreshTokens

                .Where(rt => rt.UserID == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)

                .ToListAsync();

            if (!tokens.Any())

                return Ok("No active tokens found for this user.");

            foreach (var token in tokens)

                token.IsRevoked = true;

            await _dbContext.SaveChangesAsync();

            return Ok("All active tokens revoked for this user.");

        }

    }

}

