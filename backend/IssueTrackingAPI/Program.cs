using IssueTrackingAPI.Context;
using IssueTrackingAPI.Middleware;
using IssueTrackingAPI.Repository.AttachmentRepo.AttachmentRepo;
using IssueTrackingAPI.Repository.TicketRepo.TicketRepo;
using IssueTrackingAPI.Repository.UserRepo.UserRepo;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Service to: SQL Server Connection
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Service to: Controllers
builder.Services.AddControllers();

// Service to: Repository Interface -> Repository Classes
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<ITicketRepo, TicketRepo>();
builder.Services.AddScoped<IAttachmentRepo, AttachmentRepo>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Service to: Health Check Middleware
builder.Services.AddHealthChecks();

var app = builder.Build();


// Middleware: Global Exception
app.UseGlobalExceptionMiddleware();

// Middleware: Health Check 
app.MapHealthChecks("/health");


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

app.UseAuthorization();

app.MapControllers();

app.Run();
