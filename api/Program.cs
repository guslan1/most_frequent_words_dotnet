using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

// Create a WebApplicationBuilder instance to configure services and middleware.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Register controller services for handling HTTP requests.
builder.Services.AddEndpointsApiExplorer(); // Register services for exploring API endpoints.
builder.Services.AddSwaggerGen(); // Register Swagger services for generating API documentation.

// Configure Swagger for API documentation.
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
});

// Configure rate limiting.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("GlobalPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            "Global",
            partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100, 
                Window = TimeSpan.FromMinutes(1), 
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 50
            }));
});

// Build the WebApplication instance with all configured services and middleware.
var app = builder.Build();

// Configure the HTTP request pipeline for development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger middleware for generating Swagger JSON endpoint.
    app.UseSwaggerUI(); // Enable middleware to serve Swagger UI.
}

// Configure middleware for the HTTP request pipeline.
app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS.
app.UseRateLimiter(); // Apply rate limiting to the request pipeline.

app.MapControllers(); // Map controller routes.

app.Run();
