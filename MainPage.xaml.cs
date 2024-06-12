using Newtonsoft.Json;
using Chat.ViewModel;
using System.Net.Http.Headers;

namespace Chat
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadUsers();
        }
        private async Task LoadUsers()
        {
            try
            {
                var users = await GetUsersFromApi(Connection.Server + "user/list");
                UserListView.ItemsSource = users;
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<List<User>> GetUsersFromApi(string apiUrl)
        {
            try
            {
                var jwtToken = Preferences.Get("jwtToken", string.Empty);
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return null;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await client.GetStringAsync(apiUrl);
                var users = JsonConvert.DeserializeObject<List<User>>(response);
                return users;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item != null && e.Item is User user)
            {
                await Navigation.PushAsync(new MessagePage(user.Id, user.FullName));
                ((ListView)sender).SelectedItem = null; // Deselect the item
            }
        }

    }

}
