using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Appearance;

namespace Askart
{
    public static class ThemeHelper
    {
        private static bool started = false;

        private static string[] colorNames =
        {
            "Background", "SecondaryBackground", "PrimaryText", "SecondaryText",
            "InactiveText", "Active", "Success", "Error", "CardBackground",
            "InputBackground", "Line", "PinnedBarBackground", "PinnedBarText"
        };

        public static void Start()
        {
            if (started)
            {
                return;
            }
            started = true;

            ApplicationThemeManager.ApplySystemTheme();

            ApplyOurBrushes();

            ApplicationThemeManager.Changed += OnThemeChanged;
        }

        public static void Watch(Window window)
        {
            SystemThemeWatcher.Watch(window, Wpf.Ui.Controls.WindowBackdropType.None, false);
        }

        private static void OnThemeChanged(ApplicationTheme currentTheme, Color systemAccent)
        {
            ApplyOurBrushes();
        }

        private static void ApplyOurBrushes()
        {
            string prefix = "Light";
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark)
            {
                prefix = "Dark";
            }

            foreach (string name in colorNames)
            {
                
                Color color = (Color)Application.Current.FindResource(prefix + name + "Color");

                
                
                Application.Current.Resources[name + "Brush"] = new SolidColorBrush(color);
            }
        }
    }
}
