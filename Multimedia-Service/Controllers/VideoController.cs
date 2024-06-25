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
    public class VideoController : ControllerBase
    {
        //private readonly string _targetFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Multimedia\\Videos");
        //private readonly string _compressedFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Compressed\\Videos");
        private readonly string _targetFilePath = "Multimedia\\Videos";
        private readonly string _compressedFilePath = "Compressed\\Videos";
        private readonly MultimediaContext _context;
        private string Type;

        public VideoController(MultimediaContext context)
        {
            _context = context;
            Type = GetType().Name.Replace("Controller", "");

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
        public async Task<IActionResult> GetVideo(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            var filePath = video.CompressedPath ?? video.FilePath;
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, "video/mp4");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile video, [FromForm] string sendId, [FromForm] string receiveId)
        {
            if (video == null || video.Length == 0)
            {
                return BadRequest("No video file received.");
            }

            var fileName = FFmpeg.RandomName(20) + Path.GetExtension(video.FileName);
            var filePath = Path.Combine(_targetFilePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await video.CopyToAsync(stream);
            }

            var compressedFileName = FFmpeg.RandomName(20) + ".mp4";
            var compressedFilePath = Path.Combine(_compressedFilePath, compressedFileName);
            bool compressionSuccess = await FFmpeg.CompressVideoAsync(filePath, compressedFilePath);

            if (!compressionSuccess)
            {
                return StatusCode(500, "Error compressing video.");
            }

            await FFmpeg.AddMetadataAsync(compressedFilePath);

            var videoEntity = new Video
            {
                FileName = fileName,
                FilePath = filePath,
                CompressedPath = compressedFilePath,
                UploadDate = DateTime.Now
            };

            _context.Videos.Add(videoEntity);
            await _context.SaveChangesAsync();

            var message = new Message
            {
                SendId = int.Parse(sendId),
                ReceiveId = int.Parse(receiveId),
                Type = Type,
                Value = videoEntity.Id.ToString(),
                SentTime = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
