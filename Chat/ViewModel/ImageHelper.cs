using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.ViewModel
{
    internal class ImageHelper
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
