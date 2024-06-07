namespace Chat
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {            
                if (PasswordEntry.Text != null || PasswordEntry.Text == PasswordEntryConfirm.Text)
                {
                    var username = UsernameEntry.Text;
                    var fullname = FullnameEntry.Text;
                    var password = PasswordEntry.Text;

                    // Gửi yêu cầu đăng ký tới API
                    var client = new HttpClient();
                    var content = new MultipartFormDataContent
                    {
                        { new StringContent(username), "username" },
                        { new StringContent(fullname), "fullname" },
                        { new StringContent(password), "password" },
                        { new StringContent(PasswordEntryConfirm.Text), "confirmpassword" }
                    };

                    var response = await client.PostAsync("http://192.168.0.116:3000/user/register", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Success", "Đăng ký thành công", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", await response.Content.ReadAsStringAsync(), "OK");
                    }
                }
            } 
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Không thể kết nối tới máy chủ.", "OK");
            }
        }

        private async void LoginNav(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
