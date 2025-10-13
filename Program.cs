using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using rfid_receiver_api.Hubs;
using rfid_receiver_api.Models;
using rfid_receiver_api.Services;

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
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }

        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateLifetime = true;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    });

builder.Services.AddAuthorization();

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

// Apply database migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        //await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        // Log the error but continue - the database might not be ready yet
        Console.WriteLine($"Migration failed: {ex.Message}");
    }
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