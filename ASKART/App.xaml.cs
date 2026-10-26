using Askart.Models;
using System.Windows;

namespace Askart
{
    public partial class App : Application
    {
        
        public static User CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            
            ThemeHelper.Start();
            base.OnStartup(e);
        }
    }
}
