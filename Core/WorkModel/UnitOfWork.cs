using DataAccess;
using DataAccess.Models;
using DataAccess.Repositories;

namespace Core.WorkModel;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new GenericRepository<User>(_context);
        Contractors = new GenericRepository<Contractor>(_context);
        Customers = new GenericRepository<Customer>(_context);
        Works = new GenericRepository<Work>(_context);
        Messages = new GenericRepository<Message>(_context);
        Chats = new GenericRepository<Chat>(_context);
        Requests = new GenericRepository<Request>(_context);
    }

    public IGenericRepository<User> Users { get; }
    public IGenericRepository<Contractor> Contractors { get; }
    public IGenericRepository<Customer> Customers { get; }
    public IGenericRepository<Work> Works { get; }
    public IGenericRepository<Message> Messages { get; }
    public IGenericRepository<Chat> Chats { get; }
    public IGenericRepository<Request> Requests { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
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
