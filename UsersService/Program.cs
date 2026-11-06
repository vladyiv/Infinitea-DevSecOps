using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключение к PostgreSQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

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

// Проверка работы сервиса
app.MapGet("/", () => "UsersService is running");

// Получить всех пользователей
app.MapGet("/api/users", async (UsersDbContext db) =>
{
    Console.WriteLine("Получен запрос: GET /api/users");

    var users = await db.Users.ToListAsync();
    return Results.Ok(users);
});

// Получить пользователя по ID
app.MapGet("/api/users/{id}", async (int id, UsersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: GET /api/users/{id}");

    var user = await db.Users.FindAsync(id);

    if (user is null)
    {
        return Results.NotFound(
            new { message = "Пользователь не найден" });
    }

    return Results.Ok(user);
});

// Создать пользователя
app.MapPost("/api/users", async (User user, UsersDbContext db) =>
{
    Console.WriteLine(
        $"Получен запрос: POST /api/users для {user.Username}");

    // Проверяем, не занят ли Username
    var usernameExists = await db.Users
        .AnyAsync(u => u.Username == user.Username);

    if (usernameExists)
    {
        return Results.BadRequest(
            new { message = "Такой Username уже существует" });
    }

    // Проверяем, не занят ли Email
    var emailExists = await db.Users
        .AnyAsync(u => u.Email == user.Email);

    if (emailExists)
    {
        return Results.BadRequest(
            new { message = "Такой Email уже существует" });
    }

    // ID назначит база данных
    user.Id = 0;

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/users/{user.Id}", user);
});

// Обновить пользователя
app.MapPut("/api/users/{id}", async (
    int id,
    User updatedUser,
    UsersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: PUT /api/users/{id}");

    var user = await db.Users.FindAsync(id);

    if (user is null)
    {
        return Results.NotFound(
            new { message = "Пользователь не найден" });
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

    user.Username = updatedUser.Username;
    user.Email = updatedUser.Email;

    await db.SaveChangesAsync();

    return Results.Ok(user);
});

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

app.Run();

// Модель пользователя
class User
{
    public int Id { get; set; }

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

