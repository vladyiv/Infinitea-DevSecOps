var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var products = new List<Product>
{
    new Product(1, "Richard", "Relax Green Tea", 200),
    new Product(2, "Tess", "Banana Split Black Tea", 100),
    new Product(3, "Greenfield", "Jasmine Dream Green Tea", 150),
    new Product(4, "Curtis", "Beauty Green Vitamin Tea", 175)
};

app.MapGet("/api/products", () =>
{
    Console.WriteLine("Получен запрос: GET /api/products");

    return Results.Ok(products);
});

app.MapGet("/api/products/{id}", (int id) =>
{
    Console.WriteLine($"Получен запрос: GET /api/products/{id}");

    var product = products.FirstOrDefault(p => p.Id == id);

    if (product is null)
    {
        return Results.NotFound(new
        {
            message = "Товар не найден"
        });
    }

    return Results.Ok(product);
});

app.Run();

record Product(
    int Id,
    string Name,
    string Description,
    decimal Price
);
