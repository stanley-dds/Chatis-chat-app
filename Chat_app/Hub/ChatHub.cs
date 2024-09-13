using Chat_app;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    private readonly ChatDbContext _context;

    public ChatHub(ChatDbContext context)
    {
        _context = context;
    }

    public async Task SendMessage(string user, string message)
    {
        var msg = new Message
        {
            User = user,
            Text = message,
            Timestamp = DateTime.Now
        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();



        // Отправить сообщение всем подключенным клиентам
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
