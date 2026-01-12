using Othoba.BanglaLinkOrange;
using Othoba.BanglaLinkOrange.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Banglalink OAuth 2.0 Client
builder.Services.AddBanglalinkAuthClient(config =>
{
    var banglalinkSection = builder.Configuration.GetSection("BanglalinkOAuth");
    config.BaseUrl = banglalinkSection["BaseUrl"] ?? throw new InvalidOperationException("BaseUrl is required");
    config.ClientId = banglalinkSection["ClientId"] ?? throw new InvalidOperationException("ClientId is required");
    config.ClientSecret = banglalinkSection["ClientSecret"] ?? throw new InvalidOperationException("ClientSecret is required");
    config.Username = banglalinkSection["Username"] ?? throw new InvalidOperationException("Username is required");
    config.Password = banglalinkSection["Password"] ?? throw new InvalidOperationException("Password is required");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
