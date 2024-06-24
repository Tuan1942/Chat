using System.Globalization;

namespace Chat.ViewModel
{
    internal class MessageWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isImageMessage && isImageMessage)
            {
                return 250; 
            }
            return 200; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
