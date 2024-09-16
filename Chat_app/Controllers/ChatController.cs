using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Chat_app.Models;
using System.Threading.Tasks;

namespace Chat_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new { Message = "Message cannot be empty." });
            }

            var userName = User.Identity.Name; // Get username from token

            // Send message to all connected clients via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", userName, message);

            return Ok(new { Message = "Message sent." });
        }
    }
}
