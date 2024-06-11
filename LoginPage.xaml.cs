namespace Chat
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            try
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new System.Net.CookieContainer()
                };
                var client = new HttpClient(handler);
                var content = new MultipartFormDataContent
                {
                    { new StringContent(username), "username" },
                    { new StringContent(password), "password" }
                };

                var response = await client.PostAsync("http://192.168.0.108:3000/user/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var cookies = handler.CookieContainer.GetCookies(new Uri("http://192.168.0.108:3000"));
                    foreach (System.Net.Cookie cookie in cookies)
                    {
                        if (cookie.Name == "jwtToken")
                        {
                            Preferences.Set("jwtToken", cookie.Value);
                        }
                    }

                    await DisplayAlert("Thành công", "Đăng nhập thành công", "OK");
                    // Chuyển hướng tới trang chính hoặc dashboard
                    await Shell.Current.GoToAsync("//" + nameof(AccountPage));
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Lỗi", $"Đăng nhập không thành công: {responseContent}", "OK");
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
        private async void RegisterNav(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
