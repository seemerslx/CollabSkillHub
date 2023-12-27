using System.Linq.Expressions;

namespace DataAccess.Repositories;

public interface IGenericRepository<T> : IDisposable where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? whereExpression, params string[] includeProps);
    Task<T?> GetFirstAsync(Expression<Func<T, bool>>? firstExpression, params string[] includeProps);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}
