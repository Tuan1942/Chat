using System.Net.Http.Headers;
using Newtonsoft.Json;
using Chat.ViewModel;

namespace Chat
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var loginStatus = await LoadUserInfo();
            if (loginStatus == false)
            {
                NavButton.IsVisible = true;
                LogoutButton.IsVisible = false;
            }
            else
            {
                NavButton.IsVisible = false;
                LogoutButton.IsVisible = true;
            }
        }

        private async Task<bool> LoadUserInfo()
        {
            UsernameLabel.Text = "";
            FullNameLabel.Text = "";
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

                        // Check if the response is JSON
                        if (response.Content.Headers.ContentType.MediaType == "application/json")
                        {
                            var user = JsonConvert.DeserializeObject<User>(responseData);
                            // Hiển thị tên người dùng trong ProfilePage
                            FullNameLabel.Text = user.FullName;
                            UsernameLabel.Text = user.Username;
                            return true;
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "Invalid response format received from the server.", "OK");
                            return false;
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", await response.Content.ReadAsStringAsync(), "OK");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }

        private async void LoginNav(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private async void RegisterNav(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }

        private async void Logout(object sender, EventArgs e)
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
                    var request = new HttpRequestMessage(HttpMethod.Post, "http://192.168.0.116:3000/user/logout");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        Preferences.Remove("jwtToken");
                        OnAppearing(); 
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"HttpRequestException: {httpEx.Message}");
                await DisplayAlert("Lỗi mạng", "Không thể kết nối tới máy chủ.", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while logging out.", "OK");
            }
        }
    }
}
