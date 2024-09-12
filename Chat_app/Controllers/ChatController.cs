using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Chat_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        // GET: api/chat
        [HttpGet]
        public IActionResult GetMessages()
        {
            // БД добавлю позже
            var messages = new List<string> { "Message1", "Message2", "Message3" };
            return Ok(messages);
        }

        // POST: api/chat
        [HttpPost]
        public IActionResult PostMessage([FromBody] string message)
        {

            return Ok($"Message '{message}' received!");
        }
    }
}
