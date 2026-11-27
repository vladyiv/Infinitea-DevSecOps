var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("Frontend");

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/openapi/products/swagger/v1/swagger.json",
        "Products API");

    options.SwaggerEndpoint(
        "/openapi/users/swagger/v1/swagger.json",
        "Users API");

    options.SwaggerEndpoint(
        "/openapi/orders/swagger/v1/swagger.json",
        "Orders API");

    options.OAuthClientId("infinitea-swagger");
    options.OAuthUsePkce();
});

app.MapReverseProxy();

app.Run();
