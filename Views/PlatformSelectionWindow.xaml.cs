using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Windows.Graphics;
using Windows.UI;
using Wall_You_Need_Next_Gen.Views.AlphaCoders;

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

            // Maximize the window while preserving window controls
            if (m_appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.Maximize();
            }

            // Initialize platforms list
            List<string> platforms = new List<string>
            {
                "Alpha Coders",
                "ArtFol",
                "Artgram",
                "Artstation",
                "Backiee",
                "Behance",
                "Bing Wallpaper Archive",
                "Cara",
                "CGSociety",
                "CharacterDesignReferences",
                "DesktopNexus",
                "DeviantArt",
                "Digital Blasphemy",
                "Dribbble",
                "HDwallpapers",
                "Kuvva",
                "NewGrounds",
                "Peakpx",
                "Pexels",
                "Pixabay",
                "Pixiv",
                "Simple Desktops",
                "Unsplash",
                "Vladstudio",
                "Wallhaven",
                "Wallpaper Cave",
                "Wallpaper Engine",
                "WallpaperHub"
            };

            // Set the ItemsSource for the ItemsRepeater
            PlatformsRepeater.ItemsSource = platforms;

            // Register for ElementPrepared event to attach hover events
            PlatformsRepeater.ElementPrepared += PlatformsRepeater_ElementPrepared;
        }

        private void PlatformButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Get the platform name directly from the DataContext
                selectedPlatform = button.DataContext?.ToString();
            }

            // Special handling for Backiee and Alpha Coders platforms
            if (selectedPlatform == "Backiee" || selectedPlatform == "Alpha Coders")
            {
                // Create and show the main window for both platforms
                MainWindow mainWindow = new MainWindow();

                // Set the window to maximize
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
                Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
                appWindow.Resize(new SizeInt32(1920, 1080)); // Set to a large size

                mainWindow.Activate();

                // Navigate to the appropriate page after activation
                if (selectedPlatform == "Alpha Coders")
                {
                    // Navigate to Alpha Coders grid page
                    mainWindow.NavigationFrame.Navigate(typeof(AlphaCoders.AlphaCodersGridPage));
                }

                // Close this window
                this.Close();
            }
            else
            {
                // For other platforms, show a message that they're not implemented yet
                ShowNotImplementedMessage();
            }
        }

        private void PlatformsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is Button button)
            {
                // Add hover events to the button
                button.PointerEntered += Button_PointerEntered;
                button.PointerExited += Button_PointerExited;
            }
        }

        private void Button_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // We'll use a simpler approach without animations for now
            if (sender is Button button && button.Content is Grid grid)
            {
                // Find the HoverOverlay border in the grid
                for (int i = 0; i < grid.Children.Count; i++)
                {
                    if (grid.Children[i] is Border border && border.Name == "HoverOverlay")
                    {
                        border.Opacity = 0.2;
                        break;
                    }
                }
            }
        }

        private void Button_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // We'll use a simpler approach without animations for now
            if (sender is Button button && button.Content is Grid grid)
            {
                // Find the HoverOverlay border in the grid
                for (int i = 0; i < grid.Children.Count; i++)
                {
                    if (grid.Children[i] is Border border && border.Name == "HoverOverlay")
                    {
                        border.Opacity = 0;
                        break;
                    }
                }
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
