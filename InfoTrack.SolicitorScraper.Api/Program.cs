using InfoTrack.SolicitorScraper.Api.Middleware;
using InfoTrack.SolicitorScraper.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaCors", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["https://localhost:7159", "http://localhost:5029"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("Scrape", TimeSpan.FromMinutes(30));
});

var app = builder.Build();

await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRequestTimeouts();
app.UseHttpsRedirection();
app.UseCors("SpaCors");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;