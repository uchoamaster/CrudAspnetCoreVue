var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("vue-local", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("vue-local");

var products = new List<Product>
{
    new() { Id = 1, Name = "Teclado Mecanico", Price = 299.90m, Stock = 8 },
    new() { Id = 2, Name = "Mouse Sem Fio", Price = 149.90m, Stock = 15 }
};
var nextId = products.Max(p => p.Id) + 1;

var productsApi = app.MapGroup("/api/products");

productsApi.MapGet("/", () => Results.Ok(products.OrderBy(p => p.Id)));

productsApi.MapGet("/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

productsApi.MapPost("/", (ProductRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0 || request.Stock < 0)
    {
        return Results.BadRequest("Dados invalidos. Verifique nome, preco e estoque.");
    }

    var product = new Product
    {
        Id = nextId++,
        Name = request.Name.Trim(),
        Price = request.Price,
        Stock = request.Stock
    };

    products.Add(product);

    return Results.Created($"/api/products/{product.Id}", product);
});

productsApi.MapPut("/{id:int}", (int id, ProductRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price < 0 || request.Stock < 0)
    {
        return Results.BadRequest("Dados invalidos. Verifique nome, preco e estoque.");
    }

    var index = products.FindIndex(p => p.Id == id);
    if (index < 0)
    {
        return Results.NotFound();
    }

    products[index] = new Product
    {
        Id = id,
        Name = request.Name.Trim(),
        Price = request.Price,
        Stock = request.Stock
    };

    return Results.Ok(products[index]);
});

productsApi.MapDelete("/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
    {
        return Results.NotFound();
    }

    products.Remove(product);
    return Results.NoContent();
});

app.Run();

sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

sealed class ProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
