using AdBallApi.Application.Repositories;
using AdBallApi.Application.Services;
using AdBallApi.Infrastructure.Data;
using AdBallApi.Infrastructure.Repositories;
using AdBallApi.Infrastructure.Services;
using AdBallApi.Web.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

// ── MySQL (Dapper) ────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IDbConnectionFactory>(
    new MySqlConnectionFactory(
        builder.Configuration.GetConnectionString("DefaultConnection")!
    )
);

// ── Redis Cache ───────────────────────────────────────────────────────────────
builder.Services.AddStackExchangeRedisCache(opt =>
    opt.Configuration = builder.Configuration["Redis:ConnectionString"]);

// ── JWT ───────────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            )
        };
    });

builder.Services.AddAuthorization();

// ── Hangfire (MySQL Storage) ──────────────────────────────────────────────────
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlStorageOptions { TablesPrefix = "hangfire_" }
    ))
);
builder.Services.AddHangfireServer();

// ── Firebase Admin ────────────────────────────────────────────────────────────
var firebaseCredJson = builder.Configuration["Firebase:ServiceAccountJson"];
if (!string.IsNullOrEmpty(firebaseCredJson))
{
#pragma warning disable CS0618
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromJson(firebaseCredJson)
    });
#pragma warning restore CS0618
}

// ── HttpClient ────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<AligoSmsService>();

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdViewRepository, AdViewRepository>();
builder.Services.AddScoped<IRoundRepository, RoundRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<IWinnerRepository, WinnerRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IWithdrawalRepository, WithdrawalRepository>();
builder.Services.AddScoped<IIpLogRepository, IpLogRepository>();
builder.Services.AddScoped<IUserSuspicionScoreRepository, UserSuspicionScoreRepository>();
builder.Services.AddScoped<IFcmTokenRepository, FcmTokenRepository>();
builder.Services.AddScoped<IRoundSeedRepository, RoundSeedRepository>();

// ── Infrastructure Services ───────────────────────────────────────────────────
builder.Services.AddScoped<ISmsService, AligoSmsService>();
builder.Services.AddScoped<IBlockchainSeedService, BlockstreamSeedService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddScoped<IDrawService, DrawService>();
builder.Services.AddScoped<IFraudDetectionService, FraudDetectionService>();
builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();

// ── Controllers + OpenAPI ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── App ───────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();
app.UseKoreanIpOnly();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

// ── 정기 잡 ────────────────────────────────────────────────────────────────────
RecurringJob.AddOrUpdate<IDrawService>(
    "weekly-draw",
    s => s.ExecuteDrawAsync(),
    "0 21 * * 6"   // 매주 토요일 21시 KST (서버 UTC면 "0 12 * * 6")
);
RecurringJob.AddOrUpdate<IFraudDetectionService>(
    "daily-fraud-recompute",
    s => s.RecomputeAllScoresAsync(),
    "0 18 * * *"   // 매일 18시
);

app.MapControllers();
app.Run();
