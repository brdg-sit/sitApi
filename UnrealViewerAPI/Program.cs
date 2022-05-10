using UnrealViewerAPI.Controllers;

var builder = WebApplication.CreateBuilder(args);

const string allowedOrigins = "_allowedOrigins";
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddControllersWithViews();
    //.AddNewtonsoftJson();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOrigins,
    builder =>
    {
        builder.AllowAnyOrigin().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors(allowedOrigins);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Welcome to Notes API!");

app.Run();
