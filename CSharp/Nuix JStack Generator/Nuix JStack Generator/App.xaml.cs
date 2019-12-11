using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace Nuix_JStack_Generator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!IsAdmin)
            {
                string message = "Your current permissions don't appear to allow for this program to query process status.  Please restart this application as adminstator.";
                string caption = "Nuix JStack Generator: Needs Elevated Permissions";
                MessageBox.Show(message, caption);
                Shutdown(1);
            }
            else
            {
                MainWindow window = new MainWindow();
                window.Show();
            }
        }

        private bool IsAdmin
        {
            get
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
