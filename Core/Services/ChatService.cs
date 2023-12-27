using Core.Interfaces;
using Core.WorkModel;
using DataAccess.Models;

namespace Core.Services;

public class ChatService : IChatService
{
    public ChatService(IUnitOfWork workModel)
    {
        WorkModel = workModel;
    }

    private IUnitOfWork WorkModel { get; }

    public async Task<Chat?> CreateChatAsync(string chatName, string customerId, string contractorId, int workId)
    {
        var chat = new Chat
        {
            Name = chatName,
            Description = "",
            CustomerId = customerId,
            ContractorId = contractorId,
            WorkId = workId
        };

        await WorkModel.Chats.AddAsync(chat);
        await WorkModel.SaveChangesAsync();

        return chat;
    }
}
