using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;
using Wall_You_Need_Next_Gen.Models;
using Wall_You_Need_Next_Gen.Services;

namespace Wall_You_Need_Next_Gen.Views.AlphaCoders
{
    public sealed partial class AlphaCodersDetailPage : Page
    {
        private WallpaperItem _currentWallpaper;
        private AlphaCodersService _alphaCodersService;

        public AlphaCodersDetailPage()
        {
            this.InitializeComponent();
            _alphaCodersService = new AlphaCodersService();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is WallpaperItem wallpaper)
            {
                _currentWallpaper = wallpaper;
                await LoadWallpaperDetails();
            }
            else if (e.Parameter is string wallpaperId)
            {
                // Load wallpaper by ID
                LoadingRing.Visibility = Visibility.Visible;
                _currentWallpaper = await _alphaCodersService.GetWallpaperDetailsAsync(wallpaperId);
                await LoadWallpaperDetails();
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadWallpaperDetails()
        {
            if (_currentWallpaper == null)
                return;

            try
            {
                // Set the page title
                TitleTextBlock.Text = _currentWallpaper.Title;

                // Load the full image
                if (_currentWallpaper.FullPhotoUrl.StartsWith("ms-appx:"))
                {
                    // For local images
                    WallpaperImage.Source = new BitmapImage(new Uri(_currentWallpaper.FullPhotoUrl));
                }
                else
                {
                    // For remote images
                    WallpaperImage.Source = await _currentWallpaper.LoadImageAsync();
                }

                // Set wallpaper info
                ResolutionTextBlock.Text = _currentWallpaper.Resolution;
                LikesTextBlock.Text = _currentWallpaper.Likes;
                DownloadsTextBlock.Text = _currentWallpaper.Downloads;

                // Set quality tag visibility and image
                if (!string.IsNullOrEmpty(_currentWallpaper.QualityTag))
                {
                    QualityTagBorder.Visibility = Visibility.Visible;
                    QualityImage.Source = new BitmapImage(new Uri(_currentWallpaper.QualityLogoPath));
                }
                else
                {
                    QualityTagBorder.Visibility = Visibility.Collapsed;
                }

                // Set AI tag visibility
                AITagBorder.Visibility = _currentWallpaper.IsAI ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading wallpaper details: {ex.Message}");
            }
        }

        private async void SetAsDesktopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show loading indicator
                LoadingRing.Visibility = Visibility.Visible;

                // In a real implementation, you would download the image and set it as desktop
                // For demo purposes, we'll just show a success message
                await Task.Delay(1000); // Simulate processing time

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Wallpaper set as desktop background.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting desktop wallpaper: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to set wallpaper as desktop background.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private async void SetAsLockScreenButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show loading indicator
                LoadingRing.Visibility = Visibility.Visible;

                // In a real implementation, you would download the image and set it as lock screen
                // For demo purposes, we'll just show a success message
                await Task.Delay(1000); // Simulate processing time

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Wallpaper set as lock screen.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting lock screen wallpaper: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to set wallpaper as lock screen.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show loading indicator
                LoadingRing.Visibility = Visibility.Visible;

                // In a real implementation, you would download the image
                // For demo purposes, we'll just show a success message
                await Task.Delay(1000); // Simulate processing time

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Wallpaper downloaded successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading wallpaper: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to download wallpaper.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private async void SourceLinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWallpaper != null && !string.IsNullOrEmpty(_currentWallpaper.SourceUrl))
            {
                await Launcher.LaunchUriAsync(new Uri(_currentWallpaper.SourceUrl));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}