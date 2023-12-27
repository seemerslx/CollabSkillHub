using DataAccess.Models;

namespace Core.Interfaces;

public interface IChatService
{
    Task<Chat?> CreateChatAsync(string chatName, string customerId, string contractorId, int workId);
}
