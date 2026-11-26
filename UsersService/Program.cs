using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using StackExchange.Redis;
using Microsoft.OpenApi;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                    "http://localhost:8083/realms/infinitea/protocol/openid-connect/auth"),

                TokenUrl = new Uri(
                    "http://localhost:8083/realms/infinitea/protocol/openid-connect/token"),

                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "User profile" },
                    { "email", "User email" }
                }
            }
        }
    });

    options.AddSecurityRequirement(document =>
    new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("oauth2", document)] =
            new List<string> { "openid", "profile", "email" }
    });
});

// Подключение к PostgreSQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString));

var redisConnectionString =
    builder.Configuration["Redis:ConnectionString"];

if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException(
        "Не задан Redis:ConnectionString");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

var keycloakAuthority =
    builder.Configuration["Keycloak:Authority"];

var keycloakIssuer =
    builder.Configuration["Keycloak:Issuer"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = keycloakIssuer,

                // Аудиторию настроим отдельно
                ValidateAudience = false,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();

Console.WriteLine($"Redis подключён: {redis.IsConnected}");

app.UseAuthentication();
app.UseAuthorization();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("infinitea-swagger");
        options.OAuthUsePkce();
    });
}

// Создание базы данных и таблиц, если их ещё нет
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.EnsureCreated();
}

// Получить всех пользователей
app.MapGet("/api/users", async (UsersDbContext db) =>
{
    var users = await db.Users
        .Select(u => new { u.Id, u.Username, u.Email })
        .ToListAsync();

    return Results.Ok(users);
});

// Получить пользователя по ID
app.MapGet("/api/users/{id}", async (int id, UsersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: GET /api/users/{id}");

    var user = await db.Users
        .Where(u => u.Id == id)
        .Select(u => new
        {
            u.Id,
            u.Username,
            u.Email
        })
        .FirstOrDefaultAsync();

    if (user is null)
    {
        return Results.NotFound(
            new { message = "Пользователь не найден" });
    }

    return Results.Ok(user);
});

// Обновить пользователя
app.MapPut("/api/users/{id}", async (
    int id,
    UpdateUserRequest updatedUser,
    UsersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: PUT /api/users/{id}");

    var user = await db.Users.FindAsync(id);

    if (user is null)
    {
        return Results.NotFound(
            new { message = "Пользователь не найден" });
    }

    if (string.IsNullOrWhiteSpace(updatedUser.Username) ||
        string.IsNullOrWhiteSpace(updatedUser.Email))
    {
        return Results.BadRequest(
            new { message = "Заполните все поля" });
    }

    // Проверяем, не занят ли Username другим пользователем
    var usernameExists = await db.Users
        .AnyAsync(u => u.Username == updatedUser.Username
                    && u.Id != id);

    if (usernameExists)
    {
        return Results.BadRequest(
            new { message = "Такой Username уже существует" });
    }

    // Проверяем, не занят ли Email другим пользователем
    var emailExists = await db.Users
        .AnyAsync(u => u.Email == updatedUser.Email
                    && u.Id != id);

    if (emailExists)
    {
        return Results.BadRequest(
            new { message = "Такой Email уже существует" });
    }

    user.Username = updatedUser.Username.Trim();
    user.Email = updatedUser.Email.Trim();

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        user.Id,
        user.Username,
        user.Email
    });
})
.RequireAuthorization();

// Удалить пользователя
app.MapDelete("/api/users/{id}", async (
    int id,
    UsersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: DELETE /api/users/{id}");

    var user = await db.Users.FindAsync(id);

    if (user is null)
    {
        return Results.NotFound(
            new { message = "Пользователь не найден" });
    }

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return Results.Ok(
        new { message = $"Пользователь {user.Username} удалён" });
})
.RequireAuthorization();

// Проверить данные авторизованного пользователя
app.MapGet("/api/users/me", async (
    ClaimsPrincipal principal,
    UsersDbContext db) =>
{
    var keycloakId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = principal.FindFirst("preferred_username")?.Value;
    var email = principal.FindFirst(ClaimTypes.Email)?.Value;

    if (string.IsNullOrWhiteSpace(keycloakId))
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest(new
        {
            message = "В токене отсутствуют Username или Email"
        });
    }

    var user = await db.Users.FirstOrDefaultAsync(
        u => u.KeycloakId == keycloakId);

    if (user is null)
    {
        user = new User
        {
            KeycloakId = keycloakId,
            Username = username,
            Email = email
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    return Results.Ok(new
    {
        user.Id,
        user.KeycloakId,
        user.Username,
        user.Email
    });
})
.RequireAuthorization();

app.Run();

// Модель пользователя
class User
{
    public int Id { get; set; }

    // ID пользователя из Keycloak
    public string KeycloakId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

// Данные для обновления пользователя
class UpdateUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

// Контекст базы данных
class UsersDbContext : DbContext
{
    public UsersDbContext(
        DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}

