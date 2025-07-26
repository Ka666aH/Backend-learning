using Microsoft.EntityFrameworkCore;

namespace ToDoAPIWithDB
{
    public class EFToDoRepository : IToDoRepository
    {
        private readonly AppDbContext _context;

        public EFToDoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ToDo>> GetAllAsync() => await _context.ToDos.ToListAsync();

        public async Task<ToDo?> GetByIdAsync(Guid id) => await _context.ToDos.FindAsync(id);

        public async Task<ToDo> CreateAsync(ToDo toDo)
        {
            _context.ToDos.Add(toDo);
            await _context.SaveChangesAsync();
            return toDo;
        }

        public async Task<ToDo?> PutAsync(Guid id, ToDo toDo)
        {
            var existingToDo = await _context.ToDos.FindAsync(id);
            if (existingToDo == null) return null;

            existingToDo.Title = toDo.Title;
            existingToDo.Date = toDo.Date;
            existingToDo.Description = toDo.Description;

            await _context.SaveChangesAsync();
            return existingToDo;
        }

        public async Task<ToDo?> PatchAsync(Guid id, ToDoPatchDTO patch)
        {
            var existingToDo = await _context.ToDos.FindAsync(id);
            if (existingToDo == null) return null;

            if (!string.IsNullOrEmpty(patch.Title)) existingToDo.Title = patch.Title;
            if (patch.Date != null) existingToDo.Date = patch.Date.Value;
            if (patch.Description != null) existingToDo.Description = patch.Description;

            await _context.SaveChangesAsync();
            return existingToDo;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var toDo = await _context.ToDos.FindAsync(id);
            if (toDo == null) return false;

            _context.ToDos.Remove(toDo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
