using DataAccess.Models;
using DataAccess.Repositories;

namespace Core.WorkModel;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Contractor> Contractors { get; }
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<Work> Works { get; }
    IGenericRepository<Message> Messages { get; }
    IGenericRepository<Chat> Chats { get; }
    IGenericRepository<Request> Requests { get; }
    IGenericRepository<Review> Reviews { get; }

    Task<int> SaveChangesAsync();
}
