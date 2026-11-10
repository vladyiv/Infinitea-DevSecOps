using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключение к PostgreSQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JWT Key должен содержать минимум 32 байта");
}

var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
})

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

// Регистрация пользователя
app.MapPost("/api/auth/register", async (
    RegisterRequest request,
    UsersDbContext db,
    IPasswordHasher<User> passwordHasher) =>
{
    Console.WriteLine(
        $"Получен запрос: POST /api/auth/register для {request.Username}");

    if (string.IsNullOrWhiteSpace(request.Username) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(
            new { message = "Заполните все поля" });
    }

    if (request.Password.Length < 8)
    {
        return Results.BadRequest(
            new { message = "Пароль должен содержать минимум 8 символов" });
    }

    var usernameExists = await db.Users
        .AnyAsync(u => u.Username == request.Username);

    if (usernameExists)
    {
        return Results.BadRequest(
            new { message = "Такой Username уже существует" });
    }

    var emailExists = await db.Users
        .AnyAsync(u => u.Email == request.Email);

    if (emailExists)
    {
        return Results.BadRequest(
            new { message = "Такой Email уже существует" });
    }

    var user = new User
    {
        Username = request.Username.Trim(),
        Email = request.Email.Trim()
    };

    user.PasswordHash = passwordHasher.HashPassword(
        user,
        request.Password);

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/users/{user.Id}",
        new
        {
            user.Id,
            user.Username,
            user.Email
        });
});

// Вход в аккаунт
app.MapPost("/api/auth/login", async (
    LoginRequest request,
    UsersDbContext db,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration) =>
{
    Console.WriteLine($"Получен запрос: POST /api/auth/login для {request.Username}");

    // Ищем пользователя по Username
    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Username == request.Username);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    // Проверяем пароль
    var passwordResult = passwordHasher.VerifyHashedPassword(
        user,
        user.PasswordHash,
        request.Password);

    if (passwordResult == PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    // Получаем настройки JWT
    var jwtKey = configuration["Jwt:Key"]!;
    var issuer = configuration["Jwt:Issuer"]!;
    var audience = configuration["Jwt:Audience"]!;

    // Создаём ключ и подпись токена
    var securityKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey));

    var credentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256);

    // Добавляем информацию о пользователе в токен
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
    };

    // Создаём JWT
    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: credentials);

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new
    {
        accessToken = tokenString,
        tokenType = "Bearer",
        expiresAt = token.ValidTo
    });
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
});
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
});
.RequireAuthorization();

app.Run();

// Модель пользователя
class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
}

// Данные для регистрации
class RegisterRequest
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

// Данные для входа
class LoginRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
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

