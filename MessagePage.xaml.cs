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
        messages = new ObservableCollection<Message>();
        MessagesListView.ItemsSource = messages;
        Title = FullName;

        // Start the timer to auto-refresh messages
        StartAutoRefresh();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var currentUser = await CurrentUserInfo();
        currentUserId = currentUser.Id;
        LoadMessages(currentUser.Id, messageTo);
        Title = FullName;
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

                handler.CookieContainer.Add(new Uri("http://192.168.0.116:3000"), new System.Net.Cookie("jwtToken", jwtToken));

                var client = new HttpClient(handler);
                var request = new HttpRequestMessage(HttpMethod.Get, "http://192.168.0.116:3000/user/current");
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
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to fetch user information.", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "JWT token is missing.", "OK");
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
            var response = await client.GetStringAsync($"http://192.168.0.116:3000/Message/?sendId={sendId}&receiveId={receiveId}");
            var messageItems = JsonConvert.DeserializeObject<List<Message>>(response);

            messages.Clear();
            foreach (var message in messageItems)
            {
                messages.Add(message);
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
            await DisplayAlert("Error", "Message cannot be empty.", "OK");
            return;
        }

        try
        {
            var jwtToken = Preferences.Get("jwtToken", string.Empty);
            if (string.IsNullOrEmpty(jwtToken))
            {
                await DisplayAlert("Error", "User is not authenticated.", "OK");
                return;
            }

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            handler.CookieContainer.Add(new Uri("http://192.168.0.116:3000"), new System.Net.Cookie("jwtToken", jwtToken));

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

            var response = await client.PostAsync("http://192.168.0.116:3000/Message/SendMessage", content);

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
                await DisplayAlert("Error", $"Failed to send message: {responseContent}", "OK");
            }
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Error", $"Request error: {ex.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    private void StartAutoRefresh()
    {
        isRunning = true;
        Device.StartTimer(TimeSpan.FromSeconds(5), () =>
        {
            if (isRunning)
            {
                RefreshMessages();
                return true; // Repeat again
            }
            return false; // Stop the timer
        });
    }

    private async void RefreshMessages()
    {
        var newMessages = await FetchMessages(currentUserId, messageTo);
        if (newMessages.Count > messages.Count)
        {
            messages.Clear();
            foreach (var msg in newMessages)
            {
                messages.Add(msg);
            }
            MessagesListView.ScrollTo(newMessages.Last(), ScrollToPosition.End, true);
        }
    }

    private async Task<List<Message>> FetchMessages(int sendId, int receiveId)
    {
        try
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync($"http://192.168.0.116:3000/Message/?sendId={sendId}&receiveId={receiveId}");
            var newMessages = JsonConvert.DeserializeObject<List<Message>>(response);
            return newMessages;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load messages: {ex.Message}", "OK");
            return new List<Message>();
        }
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
