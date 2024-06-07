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
                var users = await GetUsersFromApi("http://192.168.0.116:3000/user/list");
                UserListView.ItemsSource = users;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load users.", "OK");
            }
        }

        private async Task<List<User>> GetUsersFromApi(string apiUrl)
        {
            try
            {
                var jwtToken = Preferences.Get("jwtToken", string.Empty);
                if (string.IsNullOrEmpty(jwtToken))
                {
                    await DisplayAlert("Error", "User is not authenticated.", "OK");
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
                await DisplayAlert("Error", "Failed to load users.", "OK");
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

        private async void LoadImages()
        {
            try
            {
                var images = await GetImagesFromApi("http://192.168.0.116:3000/image/all");

                foreach (var image in images)
                {
                    var imageSource = await ImageHelper.GetImageFromApi($"http://192.168.0.116:3000/image/{image.Id}");

                    if (imageSource != null)
                    {
                        var imageView = new Image
                        {
                            Source = imageSource,
                            Aspect = Aspect.AspectFit 
                        };

                        //Layout.Children.Add(imageView);
                    }
                    else
                    {
                        Console.WriteLine("Failed to load image.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading images: " + ex.Message);
            }
        }

        private async Task<List<ImageItem>> GetImagesFromApi(string apiUrl)
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetStringAsync(apiUrl);
                var imageItems = JsonConvert.DeserializeObject<List<ImageItem>>(response);
                return imageItems;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load images.", "OK");
                return null;
            }
        }
    }

    public class ImageHelper
    {
        public static async Task<ImageSource> GetImageFromApi(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] imageBytes = await client.GetByteArrayAsync(url);
                    Stream stream = new MemoryStream(imageBytes);
                    ImageSource imageSource = ImageSource.FromStream(() => stream);
                    return imageSource;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load image: " + ex.Message);
                return null;
            }
        }
    }
}
