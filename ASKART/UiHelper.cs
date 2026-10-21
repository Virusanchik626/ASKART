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


        public static TextBlock MakeText(string text, double size, string brushKey)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontFamily = GetFont();
            textBlock.FontSize = size;
            textBlock.SetResourceReference(TextBlock.ForegroundProperty, brushKey);
            return textBlock;
        }


        public static Button MakeCancelButton()
        {
            Button button = new Button();
            button.Content = "Отмена";
            button.Padding = new Thickness(15, 8, 15, 8);
            button.Margin = new Thickness(0, 0, 10, 0);
            button.Background = Brushes.Transparent;
            button.BorderThickness = new Thickness(1);
            button.Cursor = Cursors.Hand;
            button.SetResourceReference(Control.BorderBrushProperty, "SecondaryTextBrush");
            button.SetResourceReference(Control.ForegroundProperty, "PrimaryTextBrush");
            return button;
        }


        public static Button MakeMainButton(string text)
        {
            Button button = new Button();
            button.Content = text;
            button.Style = (Style)Application.Current.FindResource("StandardButtonStyle");
            button.Padding = new Thickness(15, 8, 15, 8);
            return button;
        }


        public static Button MakeIconButton(string text, double size, string tooltip)
        {
            Button button = new Button();
            button.Content = text;
            button.Background = Brushes.Transparent;
            button.BorderThickness = new Thickness(0);
            button.FontSize = size;
            button.Cursor = Cursors.Hand;
            button.Padding = new Thickness(4, 0, 4, 0);
            button.ToolTip = tooltip;
            return button;
        }
    }
}
