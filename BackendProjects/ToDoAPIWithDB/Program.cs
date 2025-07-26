using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.ExceptionServices;
using ToDoAPIWithDB;


var builder = WebApplication.CreateBuilder(args);

// Получаем строку подключения из конфигурации (appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")?
    .Replace("{DB_PASSWORD}", builder.Configuration["DB_PASSWORD"]);

// Регистрируем AppDbContext, используя провайдер Npgsql и строку подключения
builder.Services.AddDbContext<AppDbContext>(options =>options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v2", new()
    {
        Version = "v2",
        Title = "ToDo API",
        Description = "An ASP.NET Core Minimal API for managing ToDo items"
    });
});
builder.Services.AddScoped<IToDoRepository, EFToDoRepository>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.MapGet("/tasks", async (IToDoRepository repository) =>
{
    var tasks = await repository.GetAllAsync();
    return Results.Ok(tasks);
});

app.MapGet("/tasks/{id}", async (Guid id, IToDoRepository repository) =>
{
    var task = await repository.GetByIdAsync(id);
    return task is not null ? Results.Ok(task) : Results.NotFound();

});

app.MapPost("/tasks", async (ToDo toDo, IToDoRepository repository) =>
{
    if (string.IsNullOrEmpty(toDo.Title)) return Results.BadRequest("No title");
    await repository.CreateAsync(toDo);
    return Results.Created($"/tasks/{toDo.Id}", toDo);
});

app.MapPut("/tasks/{id}", async (Guid id, ToDo toDo, IToDoRepository repository) =>
{
    var task = await repository.PutAsync(id, toDo);
    return task == null? Results.NotFound() :Results.Ok(task);
});

app.MapPatch("/tasks/{id}", async (Guid id, ToDoPatchDTO patch, IToDoRepository repository) =>
{
    var task = await repository.PatchAsync(id, patch);
    return task == null ? Results.NotFound() : Results.Ok(task);
});

app.MapDelete("/tasks/{id}", async (Guid id, IToDoRepository repository) =>
{

    var task = await repository.DeleteAsync(id);
    return task == false ? Results.NotFound() : Results.NoContent();
});

app.Run();

public record ToDoPatchDTO
{
    public string? Title { get; set; }
    public DateOnly? Date { get; set; }
    public string? Description { get; set; }
}

public class ToDo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; protected set; } = Guid.NewGuid();
    [Required]
    public required string Title { get; set; }
    [Required]
    public required DateOnly Date { get; set; }
    public string? Description { get; set; }

    public ToDo(string title, DateOnly date, string? description = null)
    {
        Title = title;
        Description = description;
        Date = date;
    }
    protected ToDo() { }
}