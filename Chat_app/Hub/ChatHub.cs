using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        // Отправить сообщение всем подключенным клиентам
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
