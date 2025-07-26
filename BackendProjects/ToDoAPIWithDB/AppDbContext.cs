using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // Для DbSet<ToDo>

public class AppDbContext : DbContext
{
    // Конструктор, принимающий опции (строка подключения будет задана в Program.cs)
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSet представляет таблицу в БД для сущности ToDo
    public DbSet<ToDo> ToDos { get; set; } = null!; // Инициализация для подавления предупреждений

    // (Опционально) Можно переопределить OnModelCreating для детальной настройки модели
}