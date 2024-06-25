using Microsoft.EntityFrameworkCore;

namespace MultimediaService.Context
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    /*
    public class TokenBlacklist
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
    */
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        //public DbSet<TokenBlacklist> TokenBlacklists { get; set; }
    }
}
