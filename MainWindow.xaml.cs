using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Wall_You_Need_Next_Gen.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using Windows.Storage;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Wall_You_Need_Next_Gen
{
    /// <summary>
    /// Main application window with navigation.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow m_appWindow;
        
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Set up custom titlebar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomTitleBar);
            
            // Change the window's title
            Title = "Wall-You-Need";
            
            // Get the AppWindow for this window
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            m_appWindow = AppWindow.GetFromWindowId(windowId);
            
            // Register for window closing event
            m_appWindow.Closing += AppWindow_Closing;
            
            // Restore window position and size if available
            RestoreWindowPositionAndSize();
            
            // Navigate to the homepage by default
            ContentFrame.Navigate(typeof(HomePage));
        }
        
        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            // Save window position and size when closing
            SaveWindowPositionAndSize();
        }
        
        private void SaveWindowPositionAndSize()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            // Save position
            localSettings.Values["WindowPositionX"] = m_appWindow.Position.X;
            localSettings.Values["WindowPositionY"] = m_appWindow.Position.Y;
            
            // Save size
            localSettings.Values["WindowWidth"] = m_appWindow.Size.Width;
            localSettings.Values["WindowHeight"] = m_appWindow.Size.Height;
        }
        
        private void RestoreWindowPositionAndSize()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            // Check if we have saved position and size
            if (localSettings.Values.ContainsKey("WindowPositionX") &&
                localSettings.Values.ContainsKey("WindowPositionY") &&
                localSettings.Values.ContainsKey("WindowWidth") &&
                localSettings.Values.ContainsKey("WindowHeight"))
            {
                try
                {
                    // Restore position
                    int posX = (int)localSettings.Values["WindowPositionX"];
                    int posY = (int)localSettings.Values["WindowPositionY"];
                    m_appWindow.Move(new PointInt32(posX, posY));
                    
                    // Restore size
                    int width = (int)localSettings.Values["WindowWidth"];
                    int height = (int)localSettings.Values["WindowHeight"];
                    m_appWindow.Resize(new SizeInt32(width, height));
                }
                catch (Exception)
                {
                    // If something goes wrong, just use default size/position
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string navItemTag = args.SelectedItemContainer.Tag.ToString();
                
                // Navigation logic based on the selected tag
                switch (navItemTag)
                {
                    case "Home":
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                    case "Collections":
                    case "AIGenerated":
                    case "Personal":
                    case "Slideshow":
                    case "InteractiveSlideshow":
                    case "Widgets":
                    case "UploadWallpaper":
                    case "MyAccount":
                    case "Settings":
                        // For now, we'll just navigate to Home for all options
                        // In a real app, you would navigate to different pages
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                }
            }
        }
    }
}
