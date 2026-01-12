using Othoba.BanglaLinkOrange;
using Othoba.BanglaLinkOrange.Clients;
using WebApiExample.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Banglalink Loyalty API Example",
        Version = "v1",
        Description = "Example Web API demonstrating usage of Banglalink Auth and Loyalty clients"
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ✅ Register ALL Banglalink services with a single configuration
builder.Services.AddBanglalink(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Map health check
app.MapHealthChecks("/health");

app.MapControllers();

// Welcome endpoint
app.MapGet("/", () => new
{
    message = "Welcome to Banglalink Loyalty API Example",
    version = "v1",
    endpoints = new
    {
        swagger = "/swagger",
        health = "/health",
        memberProfile = "/api/loyalty/member-profile?msisdn=88014123456789&channel=LMSMYBLAPP",
        memberProfileDetails = "/api/loyalty/member-profile-details?msisdn=88014123456789&channel=LMSMYBLAPP"
    }
}).WithName("Welcome").WithOpenApi();

await app.RunAsync();
