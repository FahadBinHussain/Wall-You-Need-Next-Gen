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