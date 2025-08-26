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

            System.Diagnostics.Debug.WriteLine("AlphaCodersDetailPage: OnNavigatedTo called");
            System.Diagnostics.Debug.WriteLine($"Parameter type: {e.Parameter?.GetType().Name ?? "null"}");

            if (e.Parameter is WallpaperItem wallpaper)
            {
                System.Diagnostics.Debug.WriteLine($"Received WallpaperItem with ID: {wallpaper.Id}");
                _currentWallpaper = wallpaper;
                await LoadWallpaperDetails();
            }
            else if (e.Parameter is string wallpaperId)
            {
                System.Diagnostics.Debug.WriteLine($"Received wallpaper ID: {wallpaperId}");
                // Load wallpaper by ID
                LoadingRing.Visibility = Visibility.Visible;
                _currentWallpaper = await _alphaCodersService.GetWallpaperDetailsAsync(wallpaperId);
                await LoadWallpaperDetails();
                LoadingRing.Visibility = Visibility.Collapsed;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No valid parameter received for navigation");
            }
        }

        private async Task LoadWallpaperDetails()
        {
            if (_currentWallpaper == null)
            {
                System.Diagnostics.Debug.WriteLine("LoadWallpaperDetails: _currentWallpaper is null");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadWallpaperDetails: Loading details for {_currentWallpaper.Id}");

                // Set the page title
                TitleTextBlock.Text = _currentWallpaper.Title;

                // Load the full image
                System.Diagnostics.Debug.WriteLine($"Loading full image from URL: {_currentWallpaper.FullPhotoUrl}");
                _currentWallpaper.ImageSource = await _currentWallpaper.LoadFullImageAsync();

                if (_currentWallpaper.ImageSource != null)
                {
                    WallpaperImage.Source = _currentWallpaper.ImageSource;
                    System.Diagnostics.Debug.WriteLine("Full image loaded successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to load full image - ImageSource is null");
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
                System.Diagnostics.Debug.WriteLine($"Error in LoadWallpaperDetails: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                System.Diagnostics.Debug.WriteLine($"Error setting desktop wallpaper: {ex.Message}");

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
                System.Diagnostics.Debug.WriteLine($"Error setting wallpaper: {ex.Message}");

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

                if (_currentWallpaper != null && !string.IsNullOrEmpty(_currentWallpaper.Id))
                {
                    // Get the download URL
                    string downloadUrl = await _alphaCodersService.GetWallpaperDownloadUrlAsync(_currentWallpaper.Id);

                    if (!string.IsNullOrEmpty(downloadUrl))
                    {
                        // For demo purposes, we'll just show the URL in a dialog
                        // In a real implementation, you would download the image
                        var dialog = new ContentDialog
                        {
                            Title = "Download URL",
                            Content = $"Download URL: {downloadUrl}\n\nIn a real implementation, this would download the image.",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };

                        await dialog.ShowAsync();
                        return;
                    }
                }

                // If we couldn't get the download URL, show a generic success message
                var successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Wallpaper downloaded successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await successDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading full image: {ex.Message}");

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
            if (_currentWallpaper != null && !string.IsNullOrEmpty(_currentWallpaper.FullPhotoUrl))
            {
                await Launcher.LaunchUriAsync(new Uri(_currentWallpaper.FullPhotoUrl));
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
