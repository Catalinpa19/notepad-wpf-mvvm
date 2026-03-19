using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WpfApp2
{
    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance { get; } = new HeaderToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = "Images/101671.png";

            if (value is DirectoryItemType type)
            {
                switch (type)
                {
                    case DirectoryItemType.Drive:
                        imagePath = "Images/images.png";
                        break;
                    case DirectoryItemType.Folder:
                        imagePath = "Images/download.png";
                        break;
                    case DirectoryItemType.File:
                        imagePath = "Images/101671.png";
                        break;
                }
            }

            return new BitmapImage(new Uri($"pack://application:,,,/{imagePath}", UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}