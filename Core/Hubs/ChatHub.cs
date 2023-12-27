using Core.DTOs;
using Core.WorkModel;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Core.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public ChatHub(IUnitOfWork workModel)
    {
        WorkModel = workModel;
    }

    private IUnitOfWork WorkModel { get; }

    public override async Task OnConnectedAsync()
    {
        var user = await WorkModel.Users
            .GetFirstAsync(u => u.UserName == Context.User.Identity!.Name, "Chats");

        if (user is null)
            return;

        foreach (var chat in user.Chats)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chat.Name);
        }

        await base.OnConnectedAsync();
    }

    public async Task Send(MessageDto dto)
    {
        var chat = await WorkModel.Chats.GetFirstAsync(c => c.Id == dto.Id);

        if (chat is null)
            return;

        var user = await WorkModel.Users
            .GetFirstAsync(u => u.UserName == Context.User.Identity!.Name);

        if (user is null)
            return;

        var messageModel = new Message
        {
            Text = dto.Message,
            Chat = chat,
            CreatedAt = DateTime.Now,
            Sender = user.FullName
        };

        await WorkModel.Messages.AddAsync(messageModel);
        await WorkModel.SaveChangesAsync();

        messageModel.Chat = null!;

        await Clients.Group(chat.Name).SendAsync("broadcastMessage", messageModel);
    }

    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
    }
}
