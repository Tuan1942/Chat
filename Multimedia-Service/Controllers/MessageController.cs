using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultimediaService.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultimediaService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly MultimediaContext _context;
        private string Type;

        public MessageController(MultimediaContext context)
        {
            _context = context;
            Type = GetType().Name.Replace("Controller", "");
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages.ToListAsync();
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }
        public class MessageDTO
        {
            public int Id { get; set; }
            public int SendId { get; set; }
            public int ReceiveId { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public string SentTime { get; set; } // Change the type to string for formatted date
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessagesBySenderReceiver(int sendId, int receiveId)
        {
            var messages = await _context.Messages
                .Where(m => (m.SendId == sendId && m.ReceiveId == receiveId) || (m.SendId == receiveId && m.ReceiveId == sendId))
                .ToListAsync();

            var messageDTOs = messages.Select(m => new MessageDTO
            {
                Id = m.Id,
                SendId = m.SendId,
                ReceiveId = m.ReceiveId,
                Type = m.Type,
                Value = m.Value,
                SentTime = m.SentTime.ToString("HH:mm dd-MM-yyyy") // Format the date
            }).ToList();

            return Ok(messageDTOs);
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromForm] Message message)
        {
            if (message == null)
            {
                return BadRequest();
            }
            if (message.Type == null)
            {
                message.Type = Type;
            }

            message.SentTime = DateTime.Now;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null)
            {
                return BadRequest();
            }

            message.SentTime = DateTime.Now;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
