using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ClaimWise.API.Services;
using ClaimWise.Application.Interfaces;
using ClaimWise.Application.Mappings;
using ClaimWise.Application.Services;
using ClaimWise.Domain.Interfaces;
using ClaimWise.Domain.Interfaces.ClaimWise.Domain.Interfaces;
using ClaimWise.Infrastructure.Data;
using ClaimWise.Infrastructure.Repositories;
using ClaimWise.Infrastructure.Repositories.ClaimWise.Infrastructure.Repositories;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 🔹 JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            // This line sets the signing key using a symmetric key.
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// 🔹 Authorization
builder.Services.AddAuthorization();

// 🔹 Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// 🔹 Controllers & JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });



// 🔹 Custom Validation Error Formatting
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Error = e.Value.Errors.First().ErrorMessage
            });

        return new BadRequestObjectResult(errors);
    };
});

// 🔹 DbContext
builder.Services.AddDbContext<ClaimWiseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString")));

// 🔹 Background Services
builder.Services.AddHostedService<TokenCleanupService>();

// 🔹 Repositories
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IPolicyholderRepository, PolicyholderRepository>();
builder.Services.AddScoped<IDependentRepository, DependentRepository>();
builder.Services.AddScoped<IPolicyTypeRepository, PolicyTypeRepository>();
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IClaimsReportRepository, ClaimsReportRepository>();
builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();
builder.Services.AddScoped<IEligibilityCheckRepository, EligibilityCheckRepository>();
builder.Services.AddScoped<IPayoutRepository, PayoutRepository>();
builder.Services.AddScoped<IHospitalRepository, HospitalRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IClaimActionLogRepository, ClaimActionLogRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// 🔹 Services
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IPolicyholderService, PolicyholderService>();
builder.Services.AddScoped<IDependentService, DependentService>();
builder.Services.AddScoped<IPolicyTypeService, PolicyTypeService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEligibilityCheckService, EligibilityCheckService>();
builder.Services.AddScoped<IPayoutService, PayoutService>();
builder.Services.AddScoped<IClaimActionLogService, ClaimActionLogService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<JwtService>();

// 🔹 AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// 🔹 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // ✅ Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Optional if using cookies/auth
    });
});


var app = builder.Build();

// 🔹 Seeding Lance as Admin
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClaimWiseDbContext>();
    dbContext.SeedLanceAdmin();
    dbContext.SeedPratheekManager();
}

// 🔹 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 🛑 THE CRITICAL FIX IS HERE 🛑
// CORS MUST be called before UseRouting, UseAuthentication, and UseAuthorization.

app.UseCors("AllowAngularDevClient"); // MOVED HERE

app.UseHttpsRedirection();
app.UseRouting();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
