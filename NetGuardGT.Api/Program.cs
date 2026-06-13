using Microsoft.EntityFrameworkCore;
using NetGuardGT.Api.Data;
using NetGuardGT.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<NetGuardDbContext>(options =>
    options.UseSqlite("Data Source=netguard.db"));

builder.Services.AddScoped<IncidentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NetGuardDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
