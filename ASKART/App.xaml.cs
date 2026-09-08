using Askart.Models;
using System.Windows;

namespace Askart
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }

        public static bool IsDarkTheme { get; set; } = false;

        public static void ToggleTheme()
        {
            IsDarkTheme = !IsDarkTheme;
            UpdateTheme();
        }

        private static void UpdateTheme()
        {
            var resources = Current.Resources;

            if (IsDarkTheme)
            {
                resources["BackgroundBrush"] = resources["DarkBackgroundBrush"];
                resources["SecondaryBackgroundBrush"] = resources["DarkSecondaryBackgroundBrush"];
                resources["PrimaryTextBrush"] = resources["DarkPrimaryTextBrush"];
                resources["SecondaryTextBrush"] = resources["DarkSecondaryTextBrush"];
                resources["InactiveTextBrush"] = resources["DarkInactiveTextBrush"];
                resources["ActiveBrush"] = resources["DarkActiveBrush"];
                resources["SuccessBrush"] = resources["DarkSuccessBrush"];
                resources["ErrorBrush"] = resources["DarkErrorBrush"];
                resources["CardBackgroundBrush"] = resources["DarkCardBackgroundBrush"];
            }
            else
            {
                resources["BackgroundBrush"] = resources["LightBackgroundBrush"];
                resources["SecondaryBackgroundBrush"] = resources["LightSecondaryBackgroundBrush"];
                resources["PrimaryTextBrush"] = resources["LightPrimaryTextBrush"];
                resources["SecondaryTextBrush"] = resources["LightSecondaryTextBrush"];
                resources["InactiveTextBrush"] = resources["LightInactiveTextBrush"];
                resources["ActiveBrush"] = resources["LightActiveBrush"];
                resources["SuccessBrush"] = resources["LightSuccessBrush"];
                resources["ErrorBrush"] = resources["LightErrorBrush"];
                resources["CardBackgroundBrush"] = resources["LightCardBackgroundBrush"];
            }
        }
    }
}