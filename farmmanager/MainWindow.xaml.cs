using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using farmmanager.Views;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace farmmanager
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // 1. Get the native window handle (HWND) for this WinUI 3 window
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            // 2. Correctly retrieve the AppWindow instance for this window
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                // Windows 11 sizing
                appWindow.Resize(new Windows.Graphics.SizeInt32(1280, 820));
                appWindow.Title = "Plantation Manager";

                // Set title bar safely using the instance, not the class name
                if (AppWindowTitleBar.IsCustomizationSupported())
                {
                    var titleBar = appWindow.TitleBar;
                    titleBar.ExtendsContentIntoTitleBar = false;
                }
            }

            // Navigate to dashboard by default
            if (NavView.MenuItems.Count > 0)
            {
                NavView.SelectedItem = NavView.MenuItems[0];
                ContentFrame.Navigate(typeof(DashboardPage));
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                Type? pageType = item.Tag?.ToString() switch
                {
                    "dashboard" => typeof(DashboardPage),
                    "entry" => typeof(WorkEntryPage),
                    "reports" => typeof(ReportsPage),
                    "workers" => typeof(WorkersPage),
                    "parcels" => typeof(ParcelsPage),
                    "activities" => typeof(ActivitiesPage),
                    _ => null
                };

                if (pageType != null)
                    ContentFrame.Navigate(pageType);
            }
        }
    }
}
