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
        
        // Add a flag to track if we're already handling a resize operation
        private bool isHandlingResize = false;
        private SizeInt32 lastAppliedSize;
        
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
            
            // Initialize the last applied size with default minimum values
            lastAppliedSize = new SizeInt32(800, 600);
            
            // Set initial window size if needed - without triggering resize event
            if (m_appWindow.Size.Width < 800 || m_appWindow.Size.Height < 600)
            {
                isHandlingResize = true;
                try
                {
                    m_appWindow.Resize(new SizeInt32(
                        Math.Max(m_appWindow.Size.Width, 800),
                        Math.Max(m_appWindow.Size.Height, 600)));
                }
                finally
                {
                    isHandlingResize = false;
                }
            }
            
            // Register for window closing event
            m_appWindow.Closing += AppWindow_Closing;
            
            // Register for window resizing event
            m_appWindow.Changed += AppWindow_Changed;
            
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
                    // Temporarily disable resize handling during restoration
                    isHandlingResize = true;
                    
                    try
                    {
                        // Get display information to validate window position
                        // For simplicity, we'll just ensure window is not positioned negatively
                        
                        // Restore position with validation
                        int posX = (int)localSettings.Values["WindowPositionX"];
                        int posY = (int)localSettings.Values["WindowPositionY"];
                        
                        // Ensure window is not positioned off-screen
                        posX = Math.Max(posX, 0);
                        posY = Math.Max(posY, 0);
                        
                        // Apply validated position
                        m_appWindow.Move(new PointInt32(posX, posY));
                        
                        // Restore size with validation
                        int width = (int)localSettings.Values["WindowWidth"];
                        int height = (int)localSettings.Values["WindowHeight"];
                        
                        // Ensure window is not too small
                        width = Math.Max(width, 800);
                        height = Math.Max(height, 600);
                        
                        // Update last applied size
                        lastAppliedSize = new SizeInt32(width, height);
                        
                        // Temporarily unsubscribe from resize events
                        m_appWindow.Changed -= AppWindow_Changed;
                        
                        // Apply validated size
                        m_appWindow.Resize(lastAppliedSize);
                    }
                    finally
                    {
                        // Resubscribe to resize events
                        m_appWindow.Changed += AppWindow_Changed;
                        isHandlingResize = false;
                    }
                }
                catch (Exception ex)
                {
                    // If restoration fails, set to default size
                    try
                    {
                        lastAppliedSize = new SizeInt32(1024, 768);
                        m_appWindow.Changed -= AppWindow_Changed;
                        m_appWindow.Resize(lastAppliedSize);
                        m_appWindow.Changed += AppWindow_Changed;
                    }
                    catch
                    {
                        // Last resort - ignore if even this fails
                    }
                }
            }
            else
            {
                // No saved settings, use default size
                try
                {
                    lastAppliedSize = new SizeInt32(1024, 768);
                    m_appWindow.Changed -= AppWindow_Changed;
                    m_appWindow.Resize(lastAppliedSize);
                    m_appWindow.Changed += AppWindow_Changed;
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        // Add a handler for window resizing to enforce minimum size
        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // Only handle size changes
            if (args.DidSizeChange && !isHandlingResize)
            {
                // Get current size
                var currentSize = m_appWindow.Size;
                
                // Check if resize is needed
                int newWidth = Math.Max(currentSize.Width, 800);
                int newHeight = Math.Max(currentSize.Height, 600);
                
                // Only resize if necessary and different from last applied size
                if ((newWidth != currentSize.Width || newHeight != currentSize.Height) &&
                    (newWidth != lastAppliedSize.Width || newHeight != lastAppliedSize.Height))
                {
                    // Set flag to prevent reentrancy and remember this size
                    isHandlingResize = true;
                    lastAppliedSize = new SizeInt32(newWidth, newHeight);
                    
                    try
                    {
                        // Temporarily unsubscribe to prevent events while changing size
                        m_appWindow.Changed -= AppWindow_Changed;
                        m_appWindow.Resize(lastAppliedSize);
                    }
                    finally
                    {
                        // Always resubscribe and clear flag
                        m_appWindow.Changed += AppWindow_Changed;
                        isHandlingResize = false;
                    }
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
                    case "LatestWallpapers":
                        ContentFrame.Navigate(typeof(LatestWallpapersPage));
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
