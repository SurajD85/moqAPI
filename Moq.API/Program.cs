using Moq.Business;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services from the Business layer
builder.Services.AddBusinessLayer(builder.Configuration);

// Add Caching
builder.Services.AddMemoryCache();

var app = builder.Build();


// Ensure database is created and migrations are applied
app.Services.EnsureDatabaseMigrated();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
