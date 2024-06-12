using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chat.ViewModel;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;

namespace Chat;

public partial class MessagePage : ContentPage
{
    private ObservableCollection<Message> messages;
    int messageTo;
    int currentUserId;
    public string FullName;
    private bool isRunning;

    public MessagePage()
    {
        InitializeComponent();
        messages = new ObservableCollection<Message>();
        MessagesListView.ItemsSource = messages;
    }

    public MessagePage(int userId, string fullName)
    {
        InitializeComponent();
        messageTo = userId;
        FullName = fullName;
        Title = FullName;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var currentUser = await CurrentUserInfo();
        if (currentUser != null)
        {
            currentUserId = currentUser.Id;
            messages = new ObservableCollection<Message>();
            LoadMessages(currentUser.Id, messageTo);
            MessagesListView.ItemsSource = messages;
            StartAutoRefresh();
        }
        else
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Stop the auto-refresh when the page disappears
        isRunning = false;
    }

    private async Task<User> CurrentUserInfo()
    {
        try
        {
            var jwtToken = Preferences.Get("jwtToken", string.Empty);
            if (!string.IsNullOrEmpty(jwtToken))
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new System.Net.CookieContainer()
                };

                handler.CookieContainer.Add(new Uri(Connection.Server), new System.Net.Cookie("jwtToken", jwtToken));

                var client = new HttpClient(handler);
                var request = new HttpRequestMessage(HttpMethod.Get, Connection.Server + "user/current");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(responseData);
                    return user;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Failed to fetch user information.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
        }
        return null;
    }

    private async void LoadMessages(int sendId, int receiveId)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync(Connection.Server + "Message/?sendId=" + sendId + "&receiveId=" + receiveId);
            var messageItems = JsonConvert.DeserializeObject<List<Message>>(response);

            if (messageItems.Count > messages.Count)
            {
                messages.Clear();
                foreach (var message in messageItems)
                {
                    message.IsIncoming = message.SendId != currentUserId;
                    messages.Add(message);
                }
                MessagesListView.ScrollTo(messages.Last(), ScrollToPosition.End, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading messages: " + ex.Message);
        }
    }

    private async void OnSendButtonClicked(object sender, EventArgs e)
    {
        var messageText = MessageEntry.Text;

        if (string.IsNullOrEmpty(messageText))
        {
            await DisplayAlert("Lỗi", "Message cannot be empty.", "OK");
            return;
        }

        try
        {
            var jwtToken = Preferences.Get("jwtToken", string.Empty);
            if (string.IsNullOrEmpty(jwtToken))
            {
                await DisplayAlert("Lỗi", "Yêu cầu đăng nhập.", "OK");
                return;
            }

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            handler.CookieContainer.Add(new Uri(Connection.Server), new System.Net.Cookie("jwtToken", jwtToken));

            var client = new HttpClient(handler);
            var message = new MessageSendModel
            {
                SendId = currentUserId,
                ReceiveId = messageTo,
                Type = "Message",
                Value = messageText,
            };

            var json = JsonConvert.SerializeObject(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Connection.Server + "Message/SendMessage", content);

            if (response.IsSuccessStatusCode)
            {
                var msg = new Message
                {
                    SendId = currentUserId,
                    ReceiveId = messageTo,
                    Type = "Message",
                    Value = messageText,
                    SentTime = DateTime.Now.ToString()
                };
                messages.Add(msg);
                MessageEntry.Text = string.Empty;
                MessagesListView.ScrollTo(msg, ScrollToPosition.End, true);
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Lỗi", $"Failed to send message: {responseContent}", "OK");
            }
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Lỗi", $"Request error: {ex.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    private void StartAutoRefresh()
    {
        isRunning = true;
        Device.StartTimer(TimeSpan.FromSeconds(5), () =>
        {
            if (isRunning)
            {
                LoadMessages(currentUserId, messageTo);
                return true; // Repeat again
            }
            return false; // Stop the timer
        });
    }

    public class MessageSendModel
    {
        public int Id { get; set; }
        public int SendId { get; set; }
        public int ReceiveId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public DateTime SentTime { get; set; }
    }
}
