using Chat.ViewModel;

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

                    var response = await client.PostAsync(Connection.Server + "user/register", content);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Thành công", "Đăng ký thành công", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Lỗi", await response.Content.ReadAsStringAsync(), "OK");
                    }
                }
            } 
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không thể kết nối tới máy chủ.", "OK");
            }
        }

        private async void LoginNav(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
