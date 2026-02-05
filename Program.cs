using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Config;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.Services;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

var settings = new AppSettings();
builder.Services.AddSingleton(settings);

builder.Environment.EnvironmentName = settings.Environment;

builder.Services.AddDbContext<PostgresContext>(options =>
    options.UseNpgsql(settings.PostgresConnectionString));

builder.Services.AddScoped<SqlServerDataReader>();
builder.Services.AddScoped<DataSyncService>();
builder.Services.AddScoped<LayoutCalculationService>();
builder.Services.AddScoped<AreaCalculationService>();
builder.Services.AddScoped<FabricPieceService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Carpet Ops System API",
        Version = "v1",
        Description = "Fabric layout optimization system API"
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(settings.Port);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PostgresContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
    }
    catch
    {
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

Console.WriteLine($"Carpet Ops System starting on port {settings.Port}...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Swagger UI: http://localhost:{settings.Port}/swagger");

app.Run();
