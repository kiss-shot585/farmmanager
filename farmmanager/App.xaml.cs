using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using farmmanager.Data;
using farmmanager.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace farmmanager
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();

            // 2. Fire and forget the database setup safely in the background 
            // to prevent blocking the initial window presentation.
            _ = InitializeDatabaseAsync();
        }

        private async System.Threading.Tasks.Task InitializeDatabaseAsync()
        {
            try
            {
                // Initializes your SQLite schema safely
                await DatabaseInitializer.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                // Logs any database schema initialization problems straight to the VS Output Window
                System.Diagnostics.Debug.WriteLine($"DATABASE INITIALIZATION FAILURE: {ex.Message}");
            }
        }
    }
}
