using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebsocketCollabServer.Services;

namespace WebsocketCollabServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebsocketController : ControllerBase
    {
        private WebSocketServerManager _manager;

        public WebsocketController(WebSocketServerManager manager)
        {
            _manager = manager;
        }

        [HttpGet("")]
        public List<string> Get([FromQuery] string roomId)
        {
            return _manager.GetUsers(roomId);
        }
    }
}
