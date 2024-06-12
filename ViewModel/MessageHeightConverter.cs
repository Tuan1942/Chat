using System.Globalization;

namespace Chat.ViewModel
{
    internal class MessageHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isImageMessage && isImageMessage)
            {
                return 250; // Height for image messages
            }
            return 100; // Default height for text messages
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
