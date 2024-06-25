using Microsoft.EntityFrameworkCore;

namespace MultimediaService.Context
{
    public class Multimedia
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string CompressedPath { get; set; }
        public DateTime UploadDate { get; set; }
    }
    public class Image : Multimedia;
    public class Audio : Multimedia;
    public class Video : Multimedia;
    public class Message
    {
        public int Id { get; set; }
        public int SendId { get; set; }
        public int ReceiveId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public DateTime SentTime { get; set; }
    }
    public class MultimediaContext : DbContext
    {
        public MultimediaContext(DbContextOptions<MultimediaContext> options) : base(options) { }
        public DbSet<Audio> Audios { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
