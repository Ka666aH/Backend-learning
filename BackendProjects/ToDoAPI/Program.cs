using System.ComponentModel.DataAnnotations;
using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.JsonPatch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
//builder.Services.AddJsonPatch();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Minimal API for managing ToDo items"
    });
});
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

List<ToDo> toDos = new List<ToDo>();

app.MapGet("/tasks", () =>
{
    return Results.Ok(toDos);
});

app.MapGet("/tasks/{id}", (Guid id) =>
{
    var task = toDos.FirstOrDefault(t => t.Id == id);
    return task is not null ? Results.Ok(task) : Results.NotFound();

});

app.MapPost("/tasks", (ToDo toDo) =>
{
    if (string.IsNullOrEmpty(toDo.Title) /*|| toDo.Date == null*/) return Results.BadRequest("No title");
    toDos.Add(toDo);
    return Results.Created($"/tasks/{toDo.Id}", toDo);
});

app.MapPut("/tasks/{id}", (Guid id, ToDo toDo) =>
{
    ToDo? updatingToDo = toDos.FirstOrDefault(x => x.Id == id);
    if (updatingToDo == null) return Results.NotFound();
    updatingToDo.Title = toDo.Title;
    updatingToDo.Date = toDo.Date;
    updatingToDo.Description = toDo.Description;
    return Results.Ok(updatingToDo);
});

//Вариант 1 JsonPatchDocument – НЕ РАБОТАЕТ
//app.MapPatch("/tasks/v1/{id}", (Guid id, JsonPatchDocument<ToDoPatchDTO> patchDoc) =>
//{
//    ToDo? updatingToDo = toDos.FirstOrDefault(x => x.Id == id);
//    if (updatingToDo == null) return Results.NotFound();

//    var toDoPatch = new ToDoPatchDTO
//    {
//        Title = updatingToDo.Title,
//        Date = updatingToDo.Date,
//        Description = updatingToDo.Description
//    };
//    // Применяем патч
//    patchDoc.ApplyTo(toDoPatch);

//    // Обновляем только те поля, которые были в патче
//    if (toDoPatch.Title != null)
//        updatingToDo.Title = toDoPatch.Title;

//    if (toDoPatch.Date.HasValue)
//        updatingToDo.Date = toDoPatch.Date.Value;

//    if (toDoPatch.Description != null)
//        updatingToDo.Description = toDoPatch.Description;

//    return Results.Ok(updatingToDo);
//});

//Вариант 2 Ручной
app.MapPatch("/tasks/v2/{id}", (Guid id, ToDoPatchDTO patch) =>
{
    ToDo? updatingToDo = toDos.FirstOrDefault(x => x.Id == id);
    if (updatingToDo == null) return Results.NotFound();
    if (!string.IsNullOrEmpty(patch.Title)) updatingToDo.Title = patch.Title;
    if (patch.Date != null) updatingToDo.Date = patch.Date.Value;
    if (patch.Description != null) updatingToDo.Description = patch.Description;
    return Results.Ok(updatingToDo);
});

app.MapDelete("/tasks/{id}", (Guid id) =>
{
    var toDo = toDos.SingleOrDefault(t => t.Id == id);
    if (toDo == null) return Results.NotFound();
    toDos.Remove(toDo);
    return Results.NoContent();
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
    public Guid Id { get; private set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required DateOnly Date { get; set; }
    public string? Description { get; set; }

    public ToDo(string title, DateOnly date, string? description = null)
    {
        Title = title;
        Description = description;
        Date = date;
    }
    private ToDo() { }
}
//public record ToDo(Guid Id, string Title, DateOnly Date, string? Description)
//{
//    public ToDo(string title, DateOnly date, string? description = null)
//        : this(Guid.NewGuid(), title, date, description) { }
//}