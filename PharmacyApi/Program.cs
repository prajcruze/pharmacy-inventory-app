using PharmacyApi.Models;
using PharmacyApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// JSON file stores (server-side storage)
var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
builder.Services.AddSingleton(new JsonFileStore<Medicine>(Path.Combine(dataDir, "medicines.json")));
builder.Services.AddSingleton(new JsonFileStore<Sale>(Path.Combine(dataDir, "sales.json")));

// Application services
builder.Services.AddSingleton<IMedicineService, MedicineService>();
builder.Services.AddSingleton<ISaleService, SaleService>();

// CORS so the JS frontend can call the API
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   // OpenAPI endpoint at /openapi/v1.json
    app.MapScalarApiReference(); // Scalar API reference at /scalar/v1
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
