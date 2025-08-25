using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Windows.Graphics;
using Windows.UI;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class PlatformSelectionWindow : Window
    {
        private AppWindow m_appWindow;
        private string selectedPlatform;

        public PlatformSelectionWindow()
        {
            this.InitializeComponent();

            // Set up custom titlebar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomTitleBar);

            // Get the AppWindow for this window
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            m_appWindow = AppWindow.GetFromWindowId(windowId);

            // Set window size
            m_appWindow.Resize(new SizeInt32(800, 600));

            // Initialize platforms list
            List<string> platforms = new List<string>
            {
                "Backiee",
                "Alpha Coders",
                "Artstation",
                "DeviantArt",
                "Behance",
                "Pixiv",
                "Dribbble",
                "CGSociety",
                "Artgram",
                "NewGrounds",
                "ArtFol",
                "Cara",
                "CharacterDesignReferences",
                "Wallhaven",
                "DesktopNexus",
                "HDwallpapers",
                "Simple Desktops",
                "Wallpaper Cave",
                "Bing Wallpaper Archive",
                "Vladstudio",
                "Digital Blasphemy",
                "Wallpaper Engine",
                "Kuvva",
                "Unsplash",
                "Pexels",
                "Peakpx",
                "WallpaperHub",
                "Pixabay"
            };

            // Set the ItemsSource for the ItemsRepeater
            PlatformsRepeater.ItemsSource = platforms;
        }

        private void PlatformButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is StackPanel stackPanel)
            {
                // Find the TextBlock in the StackPanel
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock textBlock)
                    {
                        selectedPlatform = textBlock.Text;
                        break;
                    }
                }
            }

            // Special handling for Backiee platform
            if (selectedPlatform == "Backiee")
            {
                // Create and show the main window
                MainWindow mainWindow = new MainWindow();
                mainWindow.Activate();

                // Close this window
                this.Close();
            }
            else
            {
                // For other platforms, show a message that they're not implemented yet
                ShowNotImplementedMessage();
            }
        }

        private async void ShowNotImplementedMessage()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Platform Not Available",
                Content = $"The {selectedPlatform} platform is not implemented yet. Please select Backiee for now.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}