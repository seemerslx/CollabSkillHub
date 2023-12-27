using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? whereExpression = null,
        params string[] includeProps)
    {
        IQueryable<T> query = _dbSet;

        if (whereExpression is not null)
            query = query.Where(whereExpression);

        query = includeProps.Aggregate(query, (current, includeProperty)
            => current.Include(includeProperty));

        return await query.ToListAsync();
    }

    public async Task<T?> GetFirstAsync(Expression<Func<T, bool>>? firstExpression = null,
        params string[] includeProps)
    {
        IQueryable<T> query = _dbSet;

        if (firstExpression is not null)
            query = query.Where(firstExpression);

        query = includeProps.Aggregate(query, (current, includeProperty)
            => current.Include(includeProperty));

        return await query.FirstOrDefaultAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await Task.Run(() => _dbSet.Update(entity));
        return entity;
    }

    public async Task DeleteAsync(T entity)
    {
        await Task.Run(() => _dbSet.Remove(entity));
    }

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
                _context.Dispose();

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
