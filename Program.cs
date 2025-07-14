using Microsoft.EntityFrameworkCore;
using rfid_receiver_api.Middleware;
using rfid_receiver_api.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using rfid_receiver_api.Hubs;
using rfid_receiver_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true; // optional
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(opts =>
{
    opts.AddPolicy("DevCors", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod()
            // SignalR uses credentials (cookies / WebSockets)
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
     {
         c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
         c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
             ValidAudience = builder.Configuration["Auth0:Audience"],
             ValidIssuer = $"{builder.Configuration["Auth0:Domain"]}"
         };
     });

builder.Services.AddAuthorizationBuilder()
                .AddPolicy("todo:read-write", p => p.RequireAuthenticatedUser().RequireClaim("scope", "todo:read-write"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseLazyLoadingProxies().UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddSignalR();

builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IRfidService, RfidService>();

builder.Services.AddSingleton<RfidMovementMonitor>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RfidMovementMonitor>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await SeedData.EnsureSeededAsync(app.Services);

app.UseRouting();
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("DevCors");
app.MapHub<RfidHub>("/rfidHub");

app.MapControllers();
app.Run();