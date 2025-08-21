namespace GK.Talks.Core.Interfaces;
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByEmailAsync(string email);
    IQueryable<T> GetAll();
    Task<T> AddAsync(T entity);
    Task SaveChangesAsync();
}