using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Chat.ViewModel;

public class LoginViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private string username;
    public string Username
    {
        get => username;
        set
        {
            username = value;
            OnPropertyChanged();
        }
    }

    private string password;
    public string Password
    {
        get => password;
        set
        {
            password = value;
            OnPropertyChanged();
        }
    }

    private string message;
    public string Message
    {
        get => message;
        set
        {
            message = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new Command(async () => await LoginAsync());
    }

    private async Task LoginAsync()
    {
        using (var client = new HttpClient())
        {
            var loginData = new
            {
                UserName = Username,
                Password = Password
            };

            HttpResponseMessage response = await client.PostAsJsonAsync(Connection.Server + "user/loginAsync", loginData);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                Message = "Đăng nhập thành công!";
                // Handle token (e.g., save it for future requests)
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = JObject.Parse(errorContent)["message"]?.ToString() ?? "Đăng nhập không thành công";
                Message = errorMessage;
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
