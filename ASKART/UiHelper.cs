using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Askart
{



    public static class UiHelper
    {
        public static FontFamily GetFont()
        {
            return (FontFamily)Application.Current.FindResource("Roboto");
        }

        public static Brush GetBrush(string key)
        {
            return (Brush)Application.Current.FindResource(key);
        }

        public static BitmapImage GetLogo()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/logo.png", UriKind.Absolute));
        }


        public static Color HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return Color.FromRgb(150, 150, 150);
            }
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public static Brush HexToBrush(string hex)
        {
            return new SolidColorBrush(HexToColor(hex));
        }


    }
}
