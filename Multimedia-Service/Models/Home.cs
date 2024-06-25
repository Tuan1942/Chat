using MultimediaService.Context;

namespace MultimediaService.Models
{
    public class Home
    {
        public List<User> Users { get; set; } = new List<User>();
        public IHttpContextAccessor HttpContextAccessor { get; set; } = new HttpContextAccessor();
    }
}
