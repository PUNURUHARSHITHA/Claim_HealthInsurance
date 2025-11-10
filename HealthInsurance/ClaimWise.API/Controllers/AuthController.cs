using System.Security.Cryptography;

using ClaimWise.API.Services;

using ClaimWise.Application.DTOs;

using ClaimWise.Application.Interfaces;

using ClaimWise.Domain.Entities;

using ClaimWise.Infrastructure.Data;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;

using System.Text;

using JwtClaim = System.Security.Claims.Claim;

using System.Security.Claims;

namespace ClaimWise.API.Controllers

{

    [ApiController]

    [Route("api/[controller]")]

    public class AuthController : ControllerBase

    {
        //  Dependencies and constants

        private readonly ClaimWiseDbContext _context;

        private readonly IConfiguration _configuration;

        private readonly IRefreshTokenService _refreshTokenService;

        private const string LanceSecretId = "LANCE-UNIQUE-2025-SECRET";
        //constructor injections
        public AuthController(

            ClaimWiseDbContext context,

            IConfiguration configuration,

            IRefreshTokenService refreshTokenService)

        {

            _context = context;

            _configuration = configuration;

            _refreshTokenService = refreshTokenService;

        }



        [EnableRateLimiting("LoginPolicy")]

        [AllowAnonymous]

        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginDto dto)

        {

            // ⛔ Check for recent failed login attempts (last 10 minutes)

            var recentFailures = await _context.LoginAuditLogs

                .Where(l => l.Username == dto.Username && !l.IsSuccessful)

                .Where(l => l.Timestamp >= DateTime.UtcNow.AddMinutes(-10))

                .CountAsync();

            if (recentFailures >= 5)

            {

                await LogAudit(dto.Username, false, null);

                return Forbid("Too many failed login attempts. Please try again later.");

            }

            // 🔍 Find user and verify password

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            var isValidUser = user != null && VerifyPassword(dto.Password, user.PasswordHash);

            if (!isValidUser)

            {

                await LogAudit(dto.Username, false, null);

                return Unauthorized("Invalid credentials");

            }

            // 🔐 Restrict Admin login to Lance only

            if (user.Role == "Admin" && user.Username != "Lance")

            {

                await LogAudit(dto.Username, false, user.UserID);

                return Unauthorized("Only Lance is allowed to log in as Admin.");

            }

            // 🔐 Restrict Manager login to Pratheek only

            if (user.Role == "Manager" && user.Username != "Pratheek")

            {

                await LogAudit(dto.Username, false, user.UserID);

                return Unauthorized("Only Pratheek is allowed to log in as Manager.");

            }

            // ✅ Log successful login

            await LogAudit(dto.Username, true, user.UserID);

            // 🎟️ Generate access and refresh tokens

            var accessToken = GenerateToken(user);

            var refreshToken = GenerateSecureRefreshToken();

            // 💾 Save refresh token to DB

            var tokenEntity = new RefreshToken

            {

                Token = refreshToken,

                UserID = user.UserID,

                CreatedAt = DateTime.UtcNow,

                ExpiresAt = DateTime.UtcNow.AddDays(7),

                IsRevoked = false

            };

            _context.RefreshTokens.Add(tokenEntity);

            await _context.SaveChangesAsync();

            return Ok(new

            {

                accessToken,

                refreshToken

            });

        }


        // AuthController.cs

        [AllowAnonymous]
        [HttpPost("register")]
        // REGISTER ENDPOINT (Policyholder)
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // ✅ CORRECT LOGIC: Check only for the username's existence (NO BCrypt.Verify here)
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (existingUser != null)
            {
                return BadRequest($"Username '{dto.Username}' is already taken. Please choose a different username.");
            }

            // 2. Create new Policyholder user
            var user = new User
            {
                Username = dto.Username,

                // 🔑 SECURE HASHING: This ensures the password remains secure using BCrypt.
                PasswordHash = HashPassword(dto.Password),

                Role = "Policyholder"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }
        // For Manager Registration

        [Authorize]
        [HttpPost("create-manager")]
        // REGISTER MANAGER (Only Pratheek can create)
        public async Task<IActionResult> RegisterManager([FromBody] RegisterDto dto)
        {
            var creator = User.Identity?.Name;
            if (creator != "Pratheek")
                return Forbid("Only Pratheek is allowed to create Manager accounts.");

            var usersWithSameUsername = await _context.Users
                .Where(u => u.Username == dto.Username)
                .ToListAsync();

            var duplicateUser = usersWithSameUsername
                .FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(dto.Password, u.PasswordHash));

            if (duplicateUser != null)
                return BadRequest("User is already registered with the same password. Please choose a different password.");

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                Role = "Manager"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Manager registered successfully.");
        }


        [AllowAnonymous]

        [HttpPost("refresh")]

        // REFRESH TOKEN ENDPOINT
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)

        {

            var storedToken = await _context.RefreshTokens

                .Include(rt => rt.User)

                .FirstOrDefaultAsync(rt => rt.Token == dto.ExpiredToken);

            if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)

                return Unauthorized("Invalid or expired refresh token.");

            var newAccessToken = GenerateToken(storedToken.User);

            var newRefreshToken = GenerateSecureRefreshToken();

            storedToken.IsRevoked = true;

            storedToken.ReplacedByToken = newRefreshToken;

            var newTokenEntity = new RefreshToken

            {

                Token = newRefreshToken,

                UserID = storedToken.UserID,

                CreatedAt = DateTime.UtcNow,

                ExpiresAt = DateTime.UtcNow.AddDays(7),

                IsRevoked = false

            };

            _context.RefreshTokens.Add(newTokenEntity);

            await _context.SaveChangesAsync();

            return Ok(new

            {

                accessToken = newAccessToken,

                refreshToken = newRefreshToken

            });

        }

        [Authorize]

        [HttpPost("create-admin")]

        // CREATE ADMIN (Only Lance can create)
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)

        {

            var username = User.Identity?.Name;

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var uniqueId = User.FindFirst("UniqueIdentifier")?.Value;

            if (role != "Admin" || username != "Lance" || uniqueId != LanceSecretId)

                return Forbid("Only the real Lance is allowed to create Admins.");

            var existingUser = await _context.Users

                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (existingUser != null)

                return BadRequest("Username already exists.");

            var user = new User

            {

                Username = dto.Username,

                PasswordHash = HashPassword(dto.Password),

                Role = "Admin"

            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new

            {

                message = "New Admin created successfully.",

                createdUser = new

                {

                    user.Username,

                    user.Role

                }

            });

        }

        [Authorize]

        [HttpPost("logout")]
        // LOGOUT ENDPOINT
        public async Task<IActionResult> Logout([FromBody] LogoutDto dto)

        {

            var success = await _refreshTokenService.RevokeAsync(dto.RefreshToken);

            if (!success)

                return BadRequest("Token is invalid or already revoked.");

            return Ok("Logged out successfully.");

        }

        // AUDIT LOGGING METHOD
        private async Task LogAudit(string username, bool isSuccess, int? userId)
        {
            var auditLog = new LoginAuditLog
            {
                Username = username,
                IsSuccessful = isSuccess,
                UserID = userId,
                IPAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow
            };

            await _context.LoginAuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }


        private string GenerateToken(User user)

        {

            var claims = new List<JwtClaim>

            {

                new JwtClaim(ClaimTypes.NameIdentifier, user.UserID.ToString()),

                new JwtClaim(ClaimTypes.Name, user.Username),

                new JwtClaim(ClaimTypes.Role, user.Role)

            };

            if (user.Username == "Lance")

                claims.Add(new JwtClaim("UniqueIdentifier", LanceSecretId));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(

                issuer: _configuration["Jwt:Issuer"],

                audience: _configuration["Jwt:Audience"],

                claims: claims,

                expires: DateTime.UtcNow.AddHours(2),

                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private string HashPassword(string password)

        {

            return BCrypt.Net.BCrypt.HashPassword(password);

        }

        private bool VerifyPassword(string password, string hashedPassword)

        {

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);

        }

        private string GenerateSecureRefreshToken()

        {

            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);

        }

    }

}

