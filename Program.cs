using Microsoft.EntityFrameworkCore;
using hospital_time_tracker_service.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddDbContext<HospitalContext>(options =>
    options.UseSqlite("Data Source=hospital_tracker.db"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HospitalContext>();
    context.Database.EnsureCreated();
using Microsoft.EntityFrameworkCore;
using hospital_time_tracker_service.Data;
using Microsoft.Extensions.Logging; // Import for ILogger

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddDbContext<HospitalContext>(options =>
    options.UseSqlite("Data Source=hospital_tracker.db"));

var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<HospitalContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        context.Database.EnsureCreated();
        logger.LogInformation("Database created successfully.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while creating the database.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwaggerUI();
}

app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000") // Replace with your frontend URL
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
app.UseAuthorization();
app.MapControllers();
app.UseAuthorization();
app.MapControllers();

app.Run();
