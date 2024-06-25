using Microsoft.AspNetCore.Mvc;
using MultimediaService.Context;
using MultimediaService.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using static MultimediaService.Context.MultimediaContext;

namespace MultimediaService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        //private readonly string _targetFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Multimedia\\Audios");
        private readonly string _targetFilePath = "Multimedia\\Audios";
        private readonly string _compressedFilePath = "Compressed\\Audios";
        private readonly MultimediaContext _context;
        private string Type;

        public AudioController(MultimediaContext context)
        {
            _context = context;
            Type = this.GetType().Name.Replace("Controller", "");

            if (!Directory.Exists(_targetFilePath))
            {
                Directory.CreateDirectory(_targetFilePath);
            }

            if (!Directory.Exists(_compressedFilePath))
            {
                Directory.CreateDirectory(_compressedFilePath);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAudio(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio == null)
            {
                return NotFound();
            }

            var filePath = audio.CompressedPath ?? audio.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, "audio/mpeg");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAudio([FromForm] IFormFile audio, [FromForm] string sendId, [FromForm] string receiveId)
        {
            if (audio == null || audio.Length == 0)
            {
                return BadRequest("No audio file received.");
            }

            var fileName = FFmpeg.RandomName(20) + Path.GetExtension(audio.FileName);
            var filePath = Path.Combine(_targetFilePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audio.CopyToAsync(stream);
            }

            var compressedFileName = FFmpeg.RandomName(20) + Path.GetExtension(audio.FileName);
            var compressedFilePath = Path.Combine(_compressedFilePath, compressedFileName);
            FFmpeg.CompressAudio(filePath, compressedFilePath);

            var audioEntity = new Audio
            {
                FileName = fileName,
                FilePath = filePath,
                CompressedPath = compressedFilePath,
                UploadDate = DateTime.Now
            };

            _context.Audios.Add(audioEntity);
            await _context.SaveChangesAsync();

            var message = new Message
            {
                SendId = int.Parse(sendId),
                ReceiveId = int.Parse(receiveId),
                Type = Type,
                Value = audioEntity.Id.ToString(),
                SentTime = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
