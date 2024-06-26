﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chat.ViewModel;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;
using CommunityToolkit.Maui.Views;

namespace Chat;

public partial class MessagePage : ContentPage
{
    private ObservableCollection<Message> messages;
    int messageTo;
    int currentUserId;
    public string FullName;
    private bool isRunning;
    private FileResult selectedImageFile;
    private FileResult selectedVideoFile;

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
        // Dừng làm mới
        isRunning = false;
    }

    // Lấy thông tin người dùng hiện tại đăng nhập
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
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Không thể tìm thấy thông tin người dùng.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
        }
        return null;
    }

    // Tải danh sách các tin nhắn
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

    // Gửi tin nhắn văn bản
    private async void OnSendButtonClicked(object sender, EventArgs e)
    {
        var messageText = MessageEntry.Text;

        if (string.IsNullOrEmpty(messageText))
        {
            await DisplayAlert("Lỗi", "Cần nhập tin nhắn.", "OK");
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

    // Tự động làm mới mỗi 5s, nếu có tin nhắn mới, đưa lên màn hình
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

    // Chọn tệp từ bộ nhớ
    private async void OnSelectImageButtonClicked(object sender, EventArgs e)
    {
        selectedVideoFile = null;
        selectedImageFile = null;
        PreviewVideo.IsVisible = false;
        PreviewImage.IsVisible = false;
        try
        {
            string action = await DisplayActionSheet("Chọn nguồn ảnh", "Hủy", null, "Chụp ảnh mới", "Chọn từ bộ nhớ");

            if (action == "Chọn từ bộ nhớ")
            {
                selectedImageFile = await MediaPicker.PickPhotoAsync();
            }
            else if (action == "Chụp ảnh mới")
            {
                selectedImageFile = await MediaPicker.CapturePhotoAsync();
            }
            else
            {
                return;
            }

            if (selectedImageFile == null)
                return;

            using (var stream = await selectedImageFile.OpenReadAsync())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    byte[] imageData = ms.ToArray();

                    // Hiển thị xem trước hình ảnh
                    PreviewImage.IsVisible = true;
                    PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
                    PreviewRegion.IsVisible = true;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }
    private async void OnSelectVideoButtonClicked(object sender, EventArgs e)
    {
        selectedVideoFile = null;
        selectedImageFile = null;
        PreviewImage.Source = null;
        PreviewVideo.Source = null;
        var action = await DisplayActionSheet("Chọn video", "Hủy", null, "Chọn từ bộ nhớ", "Quay video mới");

        if (action == "Chọn từ bộ nhớ")
        {
            selectedVideoFile = await MediaPicker.PickVideoAsync(); // Mở danh mục video từ bộ nhớ điện thoại
        }
        else if (action == "Quay video mới")
        {
            selectedVideoFile = await MediaPicker.CaptureVideoAsync(); // Quay video mới
        }

        if (selectedVideoFile == null)
            return;

        var temporaryVideoFile = Path.Combine(FileSystem.CacheDirectory, Path.GetFileName(selectedVideoFile.FullPath));

        using (var inputStream = await selectedVideoFile.OpenReadAsync())
        using (var outputStream = File.OpenWrite(temporaryVideoFile))
        {
            await inputStream.CopyToAsync(outputStream);
        }
        PreviewVideo.Source = MediaSource.FromFile(temporaryVideoFile);
        PreviewVideo.IsVisible = true;
        PreviewRegion.IsVisible = true;
    }

    // Gửi file
    private async void OnSendFileButtonClicked(object sender, EventArgs e)
    {
        if (selectedImageFile == null && selectedVideoFile == null)
        {
            await DisplayAlert("Lỗi", "Vui lòng chọn một hình ảnh hoặc video trước khi gửi.", "OK");
            return;
        }

        if (selectedImageFile != null)
        {
            SendImage();
        }

        if (selectedVideoFile != null)
        {
            SendVideo();
        }
    }

    // Gửi tin nhắn hình ảnh
    private async void SendImage()
    {
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

            // Chuyển hình ảnh thành dạng dữ liệu byte array
            byte[] imageData;
            using (var stream = await selectedImageFile.OpenReadAsync())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    imageData = ms.ToArray();
                }
            }

            // Thêm thông tin vào request
            var formContent = new MultipartFormDataContent();
            formContent.Headers.ContentType.MediaType = "multipart/form-data";

            // Thêm hình ảnh vào request
            var imageContent = new ByteArrayContent(imageData);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            formContent.Add(imageContent, "image", selectedImageFile.FileName);

            // Thông tin người gửi
            var sendIdContent = new StringContent(currentUserId.ToString());
            formContent.Add(sendIdContent, "sendId");

            // Thông tin người nhận
            var receiveIdContent = new StringContent(messageTo.ToString());
            formContent.Add(receiveIdContent, "receiveId");

            // Gửi request tới server
            var response = await client.PostAsync(Connection.Server + "image/upload", formContent);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Thành công", "Gửi hình ảnh thành công.", "OK");
                PreviewRegion.IsVisible = false; // Ẩn hình ảnh sau khi gửi thành công
                selectedImageFile = null; // Reset
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Lỗi", $"Failed to send image: {responseContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    // Gửi tin nhắn video
    private async void SendVideo()
    {
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

            // Chuyển thành dạng dữ liệu byte array
            byte[] videoData;
            using (var stream = await selectedVideoFile.OpenReadAsync())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    videoData = ms.ToArray();
                }
            }

            // Thêm thông tin vào request
            var formContent = new MultipartFormDataContent();
            formContent.Headers.ContentType.MediaType = "multipart/form-data";

            // Thêm hình ảnh vào request
            var imageContent = new ByteArrayContent(videoData);
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("video/jpeg");
            formContent.Add(imageContent, "video", selectedVideoFile.FileName);

            // Thông tin người gửi
            var sendIdContent = new StringContent(currentUserId.ToString());
            formContent.Add(sendIdContent, "sendId");

            // Thông tin người nhận
            var receiveIdContent = new StringContent(messageTo.ToString());
            formContent.Add(receiveIdContent, "receiveId");

            // Gửi request tới server
            var response = await client.PostAsync(Connection.Server + "video/upload", formContent);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Thành công", "Gửi video thành công.", "OK");
                PreviewRegion.IsVisible = false; // Ẩn sau khi gửi thành công
                PreviewVideo.IsVisible = false;
                selectedImageFile = null; // Reset
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Lỗi", $"Failed to send video: {responseContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", $"An unexpected error occurred: {ex.Message}", "OK");
        }
    }

    private void OnCancelButtonClicked(object sender, EventArgs e)
    {
        PreviewRegion.IsVisible = false;
        selectedImageFile = null;
        selectedVideoFile = null;
    }

    // Dùng cho việc gửi tin nhắn văn bản
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
