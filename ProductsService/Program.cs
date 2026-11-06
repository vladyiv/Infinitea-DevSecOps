using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString =
        builder.Configuration["Redis:ConnectionString"];

    return ConnectionMultiplexer.Connect(connectionString!);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
Console.WriteLine($"Redis подключён: {redis.IsConnected}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; 
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ЭНДПОИНТЫ


 // Получить весь чай
app.MapGet("/api/products", async (
    AppDbContext db,
    IConnectionMultiplexer redisConnection) =>
{
    Console.WriteLine("Получен запрос: GET /api/products");

    var redis = redisConnection.GetDatabase();
    const string cacheKey = "products:all";

    // 1. Проверяем, есть ли каталог в Redis
    var cachedProducts = await redis.StringGetAsync(cacheKey);

    if (cachedProducts.HasValue)
    {
        Console.WriteLine("Каталог получен из Redis");

        var productsFromCache =
            System.Text.Json.JsonSerializer.Deserialize<List<Product>>(
                cachedProducts.ToString());

        return Results.Ok(productsFromCache);
    }

    // 2. Если кэша нет, читаем товары из PostgreSQL
    Console.WriteLine("Кэша нет — читаем PostgreSQL");

    var products = await db.Products.ToListAsync();

    // 3. Сохраняем копию списка в Redis на 5 минут
    var json = System.Text.Json.JsonSerializer.Serialize(products);

    await redis.StringSetAsync(
        cacheKey,
        json,
        TimeSpan.FromMinutes(5));

    return Results.Ok(products);
});


// Получить один чай по ID
app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>
{
    Console.WriteLine($"Получен запрос: GET /api/products/{id}");
    var product = await db.Products.FindAsync(id);
    
    if (product is null)
    {
        return Results.NotFound(new { message = "Товар не найден" });
    }
    
    return Results.Ok(product);
});

// Добавить новый чай
app.MapPost("/api/products", async (Product product, AppDbContext db, IConnectionMultiplexer redisConnection) =>
{
    Console.WriteLine($"Получен запрос: POST /api/products для чая {product.Name}");
    
    // Проверяем, нет ли уже чая с таким ID
    var exists = await db.Products.AnyAsync(p => p.Id == product.Id);
    if (exists)
    {
        return Results.BadRequest(new { message = $"Товар с ID {product.Id} уже существует" });
    }

    db.Products.Add(product);
    await db.SaveChangesAsync();
    await InvalidateProductsCache(redisConnection);
    
    // Возвращаем статус 201 Created и ссылку на созданный товар
    return Results.Created($"/api/products/{product.Id}", product);
});

// Изменить существующий чай
app.MapPut("/api/products/{id}", async (int id, Product updatedProduct, AppDbContext db, IConnectionMultiplexer redisConnection) =>
{
    Console.WriteLine($"Получен запрос: PUT /api/products/{id}");
    
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound(new { message = "Товар для обновления не найден" });
    }

    // Обновляем поля
    product.Name = updatedProduct.Name;
    product.Description = updatedProduct.Description;
    product.Price = updatedProduct.Price;

    await db.SaveChangesAsync();
    await InvalidateProductsCache(redisConnection);
    
    return Results.Ok(product);
});

// Удалить чай
app.MapDelete("/api/products/{id}", async (int id, AppDbContext db, IConnectionMultiplexer redisConnection) =>
{
    Console.WriteLine($"Получен запрос: DELETE /api/products/{id}");
    
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound(new { message = "Товар для удаления не найден" });
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    await InvalidateProductsCache(redisConnection);
    
    return Results.Ok(new { message = $"Товар {product.Name} успешно удален" });
});

async Task InvalidateProductsCache(IConnectionMultiplexer redisConnection)
{
    var redis = redisConnection.GetDatabase();
    await redis.KeyDeleteAsync("products:all");
}

app.Run();

// МОДЕЛИ И КОНТЕКСТ БД

class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public Product() { }

    public Product(int id, string name, string description, decimal price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Product> Products => Set<Product>();
}
