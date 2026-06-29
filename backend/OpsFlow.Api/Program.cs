using Microsoft.EntityFrameworkCore;
using OpsFlow.Infrastructure;
using OpsFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OpsFlow API v1");
        options.RoutePrefix = "swagger";
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Service = "OpsFlow API",
        Timestamp = DateTime.UtcNow
    });
});

app.UseHttpsRedirection();

app.Run();