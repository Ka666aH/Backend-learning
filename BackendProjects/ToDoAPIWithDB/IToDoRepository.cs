namespace ToDoAPIWithDB
{
    public interface IToDoRepository
    {
        Task<List<ToDo>> GetAllAsync();
        Task<ToDo?> GetByIdAsync(Guid id);
        Task<ToDo> CreateAsync(ToDo toDo);
        Task<ToDo?> PutAsync(Guid id, ToDo toDo);
        Task<ToDo?> PatchAsync(Guid id, ToDoPatchDTO patch);
        Task<bool> DeleteAsync(Guid id);
    }
}
