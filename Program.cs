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
