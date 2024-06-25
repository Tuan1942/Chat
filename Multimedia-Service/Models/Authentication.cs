using MultimediaService.Context;

namespace MultimediaService.Models
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
    public class AccountModel
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public AccountModel(User user)
        {
            Username = user.Username;
            FullName = user.FullName;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        public AccountModel()
        {
            Username = string.Empty;
            FullName = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}
