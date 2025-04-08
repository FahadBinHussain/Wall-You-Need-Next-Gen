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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Wall_You_Need_Next_Gen
{
    /// <summary>
    /// Main application window with navigation.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Navigate to the homepage by default
            ContentFrame.Navigate(typeof(HomePage));
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
