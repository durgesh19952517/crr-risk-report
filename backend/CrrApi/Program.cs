using CrrApi.Repositories;
using CrrApi.Services;

var builder = WebApplication.CreateBuilder(args);

var dataPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["DataPath"]!));

// Load once at startup so the first request isn't slow.
var repository = new InMemoryPositionRepository(CsvPositionLoader.Load(dataPath));

builder.Services.AddSingleton<IPositionRepository>(repository);
builder.Services.AddScoped<PositionService>();
builder.Services.AddControllers();

var app = builder.Build();

app.Logger.LogInformation("Loaded {Count} positions from {Path}", repository.Count, dataPath);

app.MapControllers();
app.Run();
