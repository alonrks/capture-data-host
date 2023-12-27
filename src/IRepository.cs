namespace CaptureDataHost;
using System.Collections.Generic;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task AddAsync(T entity);
    Task DeleteAsync(string id);

    Task HousekeepingAsync();
}