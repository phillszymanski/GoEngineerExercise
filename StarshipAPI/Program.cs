using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StarshipAPI.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // Required for cookies
    });
});

builder.Services.AddDbContextFactory<StarshipDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<StarshipSeeder>();
builder.Services.AddScoped<IStarshipSeeder, StarshipSeeder>();
builder.Services.AddScoped<IStarshipService, StarshipService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// JWT Authentication - Read from Cookie
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Read token from cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Check for token in cookie
                if (context.Request.Cookies.TryGetValue("authToken", out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed starships on startup if database is empty
using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<StarshipDbContext>>();
    using var context = contextFactory.CreateDbContext();
    
    // Seed admin user if no users exist
    if (!context.Users.Any())
    {
        var adminUser = new StarshipAPI.Models.User
        {
            Email = "admin@starshipapi.com",
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            Created = DateTime.UtcNow,
            Edited = DateTime.UtcNow
        };
        
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
        Console.WriteLine("Admin user created: admin@starshipapi.com / password");
    }
    
    // Check if starships table is empty
    if (!context.Starships.Any())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<StarshipSeeder>();
        await seeder.SeedStarshipsAsync();
        Console.WriteLine("Database seeded with starships from SWAPI");
    }
}

app.Run();
