
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Создаём базу и таблицы, если их ещё нет
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
}

// 1. Получить корзину пользователя
app.MapGet("/api/cart/{userId:int}", async (int userId, OrdersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: GET /api/cart/{userId}");

    var cart = await db.CartItems
        .Where(item => item.UserId == userId)
        .ToListAsync();

    return Results.Ok(cart);
});

// 2. Добавить товар в корзину
app.MapPost("/api/cart/{userId:int}/items",
    async (int userId, AddCartItemRequest request, OrdersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: POST /api/cart/{userId}/items");

    if (request.ProductId <= 0 || request.Quantity <= 0)
    {
        return Results.BadRequest(new
        {
            message = "ProductId и Quantity должны быть больше нуля"
        });
    }

    var item = await db.CartItems.FirstOrDefaultAsync(
        item => item.UserId == userId &&
                item.ProductId == request.ProductId);

    if (item is not null)
    {
        // Если товар уже есть в корзине, увеличиваем количество
        item.Quantity += request.Quantity;
    }
    else
    {
        item = new CartItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        db.CartItems.Add(item);
    }

    await db.SaveChangesAsync();

    return Results.Ok(item);
});

// 3. Изменить количество товара в корзине
app.MapPut("/api/cart/{userId:int}/items/{productId:int}",
    async (int userId, int productId, UpdateCartItemRequest request,
        OrdersDbContext db) =>
{
    Console.WriteLine(
        $"Получен запрос: PUT /api/cart/{userId}/items/{productId}");

    if (request.Quantity <= 0)
    {
        return Results.BadRequest(new
        {
            message = "Количество должно быть больше нуля"
        });
    }

    var item = await db.CartItems.FirstOrDefaultAsync(
        item => item.UserId == userId &&
                item.ProductId == productId);

    if (item is null)
    {
        return Results.NotFound(new
        {
            message = "Товар не найден в корзине"
        });
    }

    item.Quantity = request.Quantity;

    await db.SaveChangesAsync();

    return Results.Ok(item);
});

// 4. Удалить товар из корзины
app.MapDelete("/api/cart/{userId:int}/items/{productId:int}",
    async (int userId, int productId, OrdersDbContext db) =>
{
    Console.WriteLine(
        $"Получен запрос: DELETE /api/cart/{userId}/items/{productId}");

    var item = await db.CartItems.FirstOrDefaultAsync(
        item => item.UserId == userId &&
                item.ProductId == productId);

    if (item is null)
    {
        return Results.NotFound(new
        {
            message = "Товар не найден в корзине"
        });
    }

    db.CartItems.Remove(item);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Товар удалён из корзины" });
});

// 5. Получить заказ по ID
app.MapGet("/api/orders/{orderId:int}",
    async (int orderId, OrdersDbContext db) =>
{
    Console.WriteLine($"Получен запрос: GET /api/orders/{orderId}");

    var order = await db.Orders
        .Include(order => order.Items)
        .FirstOrDefaultAsync(order => order.Id == orderId);

    if (order is null)
    {
        return Results.NotFound(new
        {
            message = "Заказ не найден"
        });
    }

    return Results.Ok(order);
});

app.Run();


// Запросы

class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}


// Модели данных

class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
}

class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}


// Контекст базы данных

class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>()
            .HasIndex(item => new { item.UserId, item.ProductId })
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId);
    }
}

