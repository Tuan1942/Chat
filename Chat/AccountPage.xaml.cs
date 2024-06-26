﻿using System.Net.Http.Headers;
using Newtonsoft.Json;
using Chat.ViewModel;

namespace Chat
{
    public partial class AccountPage : ContentPage
    {
        User user = new User();
        public AccountPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            user = new User();
            var loginStatus = await LoadUserInfo();
            if (loginStatus == false)
            {
                UserLogedIn.IsVisible = false;
                NavButton.IsVisible = true;
            }
            else
            {
                UserLogedIn.IsVisible = true; // Nơi hiển thị thông tin người dùng
                NavButton.IsVisible = false; // Các nút điều hướng tới đăng nhập, đăng ký
            }
        }

        // Tải thông tin người dùng
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

                    handler.CookieContainer.Add(new Uri(Connection.Server), new System.Net.Cookie("jwtToken", jwtToken));

                    var client = new HttpClient(handler);
                    var request = new HttpRequestMessage(HttpMethod.Get, Connection.Server + "user/current");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();

                        // Check if the response is JSON
                        if (response.Content.Headers.ContentType.MediaType == "application/json")
                        {
                            user = JsonConvert.DeserializeObject<User>(responseData);
                            // Hiển thị tên người dùng trong ProfilePage
                            FullNameLabel.Text = user.FullName;
                            UsernameLabel.Text = user.Username;
                            FullName.Text = user.FullName;
                            return true;
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Lỗi", "Invalid response format received from the server.", "OK");
                            return false;
                        }
                    }
                    else
                    {
                        // Token vẫn còn trong bộ nhớ, nhưng đã hết hạn
                        await Application.Current.MainPage.DisplayAlert("", "Yêu cầu đăng nhập lại.", "OK");
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

                    handler.CookieContainer.Add(new Uri(Connection.Server), new System.Net.Cookie("jwtToken", jwtToken));

                    var client = new HttpClient(handler);
                    var request = new HttpRequestMessage(HttpMethod.Post, Connection.Server + "user/logout");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        Preferences.Remove("jwtToken"); // Loại bỏ token khỏi bộ nhớ
                        user = new User();
                        OnAppearing(); 
                    }
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
                await DisplayAlert("Lỗi", "An error occurred while logging out.", "OK");
            }
        }

        // Cập nhật tài khoản
        private async void Change(object sender, EventArgs e)
        {
            try
            {
                // Nguời dùng nhập cả mật khẩu, sẽ thực hiện đổi mật khẩu
                if (!string.IsNullOrEmpty(Password.Text) && Password.Text == ConfirmPassword.Text)
                {
                    var username = user.Username;
                    var fullname = FullName.Text;
                    var password = Password.Text;

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
                        var request = new HttpRequestMessage(HttpMethod.Put, Connection.Server + "user/update");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                        var content = new MultipartFormDataContent
                        {
                            { new StringContent(username), "username" },
                            { new StringContent(fullname), "fullname" },
                            { new StringContent(password), "password" },
                            { new StringContent(ConfirmPassword.Text), "confirmpassword" }
                        };

                        request.Content = content;

                        var response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            await DisplayAlert("Thành công", "Cập nhật tài khoản thành công", "OK");
                            OnAppearing();
                        }
                        else
                        {
                            await DisplayAlert("Lỗi", await response.Content.ReadAsStringAsync(), "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Lỗi", "JWT token is missing.", "OK");
                    }
                }
                // Người dùng không nhập mật khẩu, chỉ thực hiện đổi tên người dùng
                else if (string.IsNullOrEmpty(Password.Text))
                {
                    var username = user.Username;
                    var fullname = FullName.Text;
                    var password = Password.Text;

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
                        var request = new HttpRequestMessage(HttpMethod.Put, Connection.Server + "user/update");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                        var content = new MultipartFormDataContent
                        {
                            { new StringContent(username), "username" },
                            { new StringContent(fullname), "fullname" }
                        };

                        request.Content = content;

                        var response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            await DisplayAlert("Thành công", "Cập nhật tài khoản thành công", "OK");
                            OnAppearing();
                        }
                        else
                        {
                            await DisplayAlert("Lỗi", await response.Content.ReadAsStringAsync(), "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Lỗi", "JWT token is missing.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Lỗi", "Mật khẩu xác nhận không chính xác.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể kết nối tới máy chủ: {ex.Message}", "OK");
            }
        }

    }
}
