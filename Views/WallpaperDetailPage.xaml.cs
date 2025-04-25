using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml;
using Wall_You_Need_Next_Gen.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Wall_You_Need_Next_Gen.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WallpaperDetailPage : Page
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiBaseUrl = "https://atozmashprima.com/api/search-wall-papers?id=";

        public WallpaperDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Debug.WriteLine("WallpaperDetailPage.OnNavigatedTo called");

            if (e.Parameter is WallpaperItem wallpaper)
            {
                Debug.WriteLine($"Received wallpaper: ID={wallpaper.Id}, Title={wallpaper.Title}");
                Debug.WriteLine($"FullPhotoUrl={wallpaper.FullPhotoUrl}");

                // Set title at the top of the page
                TitleTextBlock.Text = wallpaper.Title ?? "Error Loading Wallpaper";

                // Set the full image using the FullPhotoUrl property
                if (!string.IsNullOrEmpty(wallpaper.FullPhotoUrl))
                {
                    try
                    {
                        Debug.WriteLine($"Setting WallpaperImage.Source to {wallpaper.FullPhotoUrl}");
                        WallpaperImage.Source = new BitmapImage(new Uri(wallpaper.FullPhotoUrl));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error setting wallpaper image: {ex.Message}");
                        // Keep using the placeholder image (already set in XAML)
                    }
                }
                else
                {
                    Debug.WriteLine("FullPhotoUrl is empty - using placeholder image");
                }

                // Set AI tag visibility based on IsAI property
                Debug.WriteLine($"IsAI: {wallpaper.IsAI}");
                AITagBorder.Visibility = wallpaper.IsAI ? Visibility.Visible : Visibility.Collapsed;

                // Set Quality tag visibility and image based on QualityTag property
                string qualityTag = wallpaper.QualityTag?.ToUpper() ?? string.Empty;
                Debug.WriteLine($"QualityTag: {qualityTag}");

                // Construct the path to the quality logo - fix the naming convention
                string qualityLogoPath = string.Empty;
                if (qualityTag == "4K" || qualityTag == "5K" || qualityTag == "8K")
                {
                    // Update to use the correct file naming pattern
                    qualityLogoPath = $"ms-appx:///Assets/{qualityTag.ToLower()}-icon.png";
                    Debug.WriteLine($"Trying quality image: {qualityLogoPath}");
                }
                
                Debug.WriteLine($"QualityLogoPath: {qualityLogoPath}");

                // Set the quality tag visibility and image source
                if (!string.IsNullOrEmpty(qualityLogoPath))
                {
                    try
                    {
                        QualityImage.Source = new BitmapImage(new Uri(qualityLogoPath));
                        QualityTagBorder.Visibility = Visibility.Visible;
                        Debug.WriteLine("Quality tag should be visible now");
                    }
                    catch (Exception ex)
                    {
                        // Log error and keep the quality tag hidden
                        Debug.WriteLine($"Error setting quality image: {ex.Message}");
                        QualityTagBorder.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // No quality tag available
                    QualityTagBorder.Visibility = Visibility.Collapsed;
                    Debug.WriteLine("No quality tag available, hiding the quality border");
                }

                // Likes/Downloads have been removed from the UI
            }
            else
            {
                TitleTextBlock.Text = "Error: Invalid wallpaper data";
                WallpaperImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-wallpaper-1000.jpg"));
                // Ensure all overlays are hidden/reset in error case
                AITagBorder.Visibility = Visibility.Collapsed;
                QualityTagBorder.Visibility = Visibility.Collapsed;
            }
        }

        // This method would normally call the API, but we're now using the passed WallpaperItem object
        private async Task LoadWallpaperDetailsAsync(int wallpaperId)
        {
            // This method is no longer needed since we're using the passed WallpaperItem
            // But keeping it here as a reference in case we need to fetch additional details later
        }
    }
} 