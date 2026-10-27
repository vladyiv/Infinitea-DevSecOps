using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

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
app.MapGet("/api/products", async (AppDbContext db) =>
{
    Console.WriteLine("Получен запрос: GET /api/products");
    var products = await db.Products.ToListAsync();
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
app.MapPost("/api/products", async (Product product, AppDbContext db) =>
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
    
    // Возвращаем статус 201 Created и ссылку на созданный товар
    return Results.Created($"/api/products/{product.Id}", product);
});

// Изменить существующий чай
app.MapPut("/api/products/{id}", async (int id, Product updatedProduct, AppDbContext db) =>
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
    return Results.Ok(product);
});

// Удалить чай
app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) =>
{
    Console.WriteLine($"Получен запрос: DELETE /api/products/{id}");
    
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound(new { message = "Товар для удаления не найден" });
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    
    return Results.Ok(new { message = $"Товар {product.Name} успешно удален" });
});

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
