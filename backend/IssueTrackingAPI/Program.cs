using IssueTrackingAPI.Context;
using IssueTrackingAPI.Hubs;
using IssueTrackingAPI.Middleware;
using IssueTrackingAPI.Repository.AttachmentRepo.AttachmentRepo;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo;
using IssueTrackingAPI.Repository.UserRepo.UserRepo;
using IssueTrackingAPI.Repository.NotificationRepo.NotificationRepo;
using IssueTrackingAPI.Repository.DashboardRepo.DashboardRepo;
using IssueTrackingAPI.Repository.KPIRepo.KPIRepo;
using IssueTrackingAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Service to: SQL Server Connection
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service to: Controllers
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Angular dev server
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Service to: Repository Interface -> Repository Classes
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ITicketRepo, TicketRepo>();
builder.Services.AddScoped<IAttachmentRepo, AttachmentRepo>();

// Service to: New Repository Classes
builder.Services.AddScoped<INotificationRepo, NotificationRepo>();
builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();
builder.Services.AddScoped<IKPIRepo, KPIRepo>();

// Service to: Notification SignalR Service
builder.Services.AddScoped<INotificationSignalRService, NotificationSignalRService>();

// Service to: SignalR
builder.Services.AddSignalR();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

// Service to: Health Check Middleware
builder.Services.AddHealthChecks();

var app = builder.Build();


// Middleware: Global Exception
app.UseGlobalExceptionMiddleware();


// Return JSON for common status codes (404, 401, 403, etc.)
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    response.ContentType = "application/json";

    var statusCode = response.StatusCode;
    var message = statusCode switch
    {
        404 => "Resource not found",
        401 => "Unauthorized access",
        403 => "Forbidden",
        _ => "An unexpected error occurred"
    };

    await response.WriteAsync(JsonSerializer.Serialize(
    new{
        status = statusCode,
        message
    }));
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub"); // SignalR Hub
app.MapHealthChecks("/health"); // Heath Check

app.Run();
