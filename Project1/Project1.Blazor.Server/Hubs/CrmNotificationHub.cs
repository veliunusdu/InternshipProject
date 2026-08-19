#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Project1.Core.Services.Interfaces;

namespace Project1.Blazor.Server.Hubs
{
    public interface ICrmNotificationClient
    {
        Task ReceiveNoteReadNotification(NoteReadNotificationEvent notification);
        Task ReceiveSystemAlert(string title, string message, string type);
    }

    public class CrmNotificationHub : Hub<ICrmNotificationClient>
    {
        public async Task BroadcastNoteRead(NoteReadNotificationEvent notification)
        {
            await Clients.Others.ReceiveNoteReadNotification(notification);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
