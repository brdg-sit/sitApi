using UnrealViewerAPI.Controllers;

var builder = WebApplication.CreateBuilder(args);

const string MyAllowSpecificOrigins = "_MyAllowSubdomainPolicy";
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
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://*.brdg.kr").AllowAnyOrigin().AllowAnyHeader()
                .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
});

builder.Services.AddControllers();

var app = builder.Build();
//app.UseHttpsRedirection();
//app.UseStaticFiles();
app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();