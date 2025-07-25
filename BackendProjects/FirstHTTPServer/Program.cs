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

// 1. ������� GET /
app.MapGet("/", () => "Hello World!");

// 2. GET /status - ���������� ������ 200 � JSON
app.MapGet("/status", () =>
{
    return Results.Ok(new { Status = "OK", Time = DateTime.UtcNow });
});

// 3. GET /echo?message=... - ������ Query ���������
app.MapGet("/echo", (string message) => $"You said: {message}");

// 4. POST /data - ������ ���� ������� (JSON)
app.MapPost("/data", async (HttpContext context) =>
{
    // ������ ���� ������� ��� ������
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    return Results.Ok($"Received: {requestBody}");
});

// 5. ��������� �������������� ������ - 404
// (Minimal API ������������� ���������� 404 ��� ����������� ������, �� ����� �������� ���������)
app.MapFallback(() => Results.NotFound("Resource not found"));
app.Run();