var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 1. Простой GET /
app.MapGet("/", () => "Hello World!");

// 2. GET /status - Возвращаем статус 200 и JSON
app.MapGet("/status", () =>
{
    return Results.Ok(new { Status = "OK", Time = DateTime.UtcNow });
});

// 3. GET /echo?message=... - Чтение Query параметра
app.MapGet("/echo", (string message) => $"You said: {message}");

// 4. POST /data - Чтение тела запроса (JSON)
app.MapPost("/data", async (HttpContext context) =>
{
    // Читаем тело запроса как строку
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    return Results.Ok($"Received: {requestBody}");
});

// 5. Обработка несуществующих роутов - 404
// (Minimal API автоматически возвращает 404 для ненайденных роутов, но можно добавить кастомный)
app.MapFallback(() => Results.NotFound("Resource not found"));
app.Run();