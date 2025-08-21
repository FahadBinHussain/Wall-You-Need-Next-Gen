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
using Microsoft.UI.Xaml.Shapes; // for Ellipse and Path shape
using Microsoft.UI.Xaml.Media; // for ImageBrush and Stretch
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage.AccessCache; // For KnownFolders and KnownFolderId
using System.IO;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using System.Collections.Generic;
using IOPath = System.IO.Path; // Use System.IO.Path for file operations with alias
using System.Linq;
using System.Reflection; // For reflection functionality
using Windows.System.UserProfile; // For wallpaper functionality
using System.Runtime.InteropServices; // For P/Invoke

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Wall_You_Need_Next_Gen.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public static class WallpaperHelper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 0x0014;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        public static bool SetWallpaper(string path)
        {
            try
            {
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    public sealed partial class WallpaperDetailPage : Page
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiBaseUrl = "https://atozmashprima.com/api/search-wall-papers?id=";
        private const string DetailApiBaseUrl = "https://backiee.com/api/wallpaper/list.php?action=detail_page_v2&wallpaper_id=";
        private WallpaperItem _currentWallpaper;

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
                _currentWallpaper = wallpaper;
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

                // Handle AI tag exactly as in LatestWallpapersPage.SetItemMetadata
                AITagBorder.Visibility = wallpaper.IsAI ? Visibility.Visible : Visibility.Collapsed;
                if (wallpaper.IsAI)
                {
                    AIImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/aigenerated-icon.png"));
                }

                // Handle quality tag exactly as in LatestWallpapersPage.SetItemMetadata
                if (!string.IsNullOrEmpty(wallpaper.QualityTag))
                {
                    QualityTagBorder.Visibility = Visibility.Visible;
                    // Set the quality image source
                    string qualityImagePath = wallpaper.QualityLogoPath;
                    Debug.WriteLine($"QualityLogoPath from model: {qualityImagePath}");
                    
                    if (!string.IsNullOrEmpty(qualityImagePath))
                    {
                        try
                        {
                            QualityImage.Source = new BitmapImage(new Uri(qualityImagePath));
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
                        // No quality logo path available
                        QualityTagBorder.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // No quality tag available
                    QualityTagBorder.Visibility = Visibility.Collapsed;
                    Debug.WriteLine("No quality tag available, hiding the quality border");
                }

                // Fetch and display the publisher details
                if (int.TryParse(wallpaper.Id, out int wallpaperId))
                {
                    LoadPublisherDetailsAsync(wallpaperId).ConfigureAwait(false);
                }
                else
                {
                    Debug.WriteLine($"Failed to parse wallpaper ID: {wallpaper.Id}");
                }
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

        private async Task LoadPublisherDetailsAsync(int wallpaperId)
        {
            try
            {
                string apiUrl = $"{DetailApiBaseUrl}{wallpaperId}";
                Debug.WriteLine($"Fetching publisher details from: {apiUrl}");

                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Publisher API Response: {jsonResponse}");

                    using JsonDocument document = JsonDocument.Parse(jsonResponse);
                    
                    // Extract publisher data and wallpaper source URL
                    if (document.RootElement.TryGetProperty("WallpaperPublisher", out JsonElement publisherElement))
                    {
                        // Check if there's a source URL in the response
                        if (document.RootElement.TryGetProperty("SourceUrl", out JsonElement sourceUrlElement) && 
                            !string.IsNullOrEmpty(sourceUrlElement.GetString()))
                        {
                            string sourceUrl = sourceUrlElement.GetString();
                            // Update the SourceUrl property of the current wallpaper
                            _currentWallpaper.SourceUrl = sourceUrl;
                            Debug.WriteLine($"Set SourceUrl to: {sourceUrl}");
                        }
                        else
                        {
                            // If no source URL in API, create one based on wallpaper ID
                            _currentWallpaper.SourceUrl = $"https://backiee.com/wallpaper/{wallpaperId}";
                            Debug.WriteLine($"Created fallback SourceUrl: {_currentWallpaper.SourceUrl}");
                        }
                        
                        // Update UI with publisher information on the UI thread
                        DispatcherQueue.TryEnqueue(() => 
                        {
                            UpdatePublisherUI(publisherElement);
                        });
                    }
                    else
                    {
                        Debug.WriteLine("Publisher info not found in API response");
                    }
                }
                else
                {
                    Debug.WriteLine($"Error fetching publisher details: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception while loading publisher details: {ex.Message}");
            }
        }

        private void UpdatePublisherUI(JsonElement publisherElement)
        {
            try
            {
                // Extract publisher details
                string publisherName = publisherElement.TryGetProperty("Name", out JsonElement nameElement) 
                    ? nameElement.GetString() : "Unknown Publisher";
                
                string gender = publisherElement.TryGetProperty("Gender", out JsonElement genderElement) 
                    ? genderElement.GetString() : "Private";
                
                string age = publisherElement.TryGetProperty("Age", out JsonElement ageElement) 
                    ? ageElement.GetString() : "Private";
                
                string country = publisherElement.TryGetProperty("Country", out JsonElement countryElement) 
                    ? countryElement.GetString() : "Private";
                
                // Publisher profile picture
                string profileImageUrl = publisherElement.TryGetProperty("ProfileImage", out JsonElement profileImageElement) 
                    ? profileImageElement.GetString() : "";

                // Statistics
                string uploads = publisherElement.TryGetProperty("Uploads", out JsonElement uploadsElement) 
                    ? uploadsElement.GetString() : "0";
                
                string likes = publisherElement.TryGetProperty("Likes", out JsonElement likesElement) 
                    ? likesElement.GetString() : "0";
                
                string followers = publisherElement.TryGetProperty("Followers", out JsonElement followersElement) 
                    ? followersElement.GetString() : "0";

                // Update UI elements with publisher information
                // Name
                var publisherNameElement = FindName("PublisherNameTextBlock") as TextBlock;
                if (publisherNameElement != null)
                {
                    publisherNameElement.Text = publisherName;
                }

                // Publisher info
                var publisherInfoElement = FindName("PublisherInfoTextBlock") as TextBlock;
                if (publisherInfoElement != null)
                {
                    publisherInfoElement.Text = $"Gender: {gender} · Age: {age} · Country: {country}";
                }

                // Statistics
                var uploadsTextBlock = FindName("UploadsTextBlock") as TextBlock;
                if (uploadsTextBlock != null)
                {
                    uploadsTextBlock.Text = uploads;
                }

                var likesTextBlock = FindName("LikesTextBlock") as TextBlock;
                if (likesTextBlock != null)
                {
                    likesTextBlock.Text = likes;
                }

                var followersTextBlock = FindName("FollowersTextBlock") as TextBlock;
                if (followersTextBlock != null)
                {
                    followersTextBlock.Text = followers;
                }

                // Profile image
                if (!string.IsNullOrEmpty(profileImageUrl))
                {
                    try
                    {
                        // Try to find the background of the ellipse
                        var profileEllipse = FindName("PublisherProfileEllipse") as Ellipse;
                        if (profileEllipse != null)
                        {
                            var imageBrush = new ImageBrush
                            {
                                ImageSource = new BitmapImage(new Uri(profileImageUrl)),
                                Stretch = Stretch.UniformToFill
                            };
                            profileEllipse.Fill = imageBrush;
                        }

                        // Hide the wolf silhouette if we have a profile image
                        var wolfPath = FindName("PublisherProfileIcon") as Microsoft.UI.Xaml.Shapes.Path;
                        if (wolfPath != null)
                        {
                            wolfPath.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error setting publisher profile image: {ex.Message}");
                        // Keep the default gradient and silhouette
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating publisher UI: {ex.Message}");
            }
        }

        private async void SetAsWallpaperButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWallpaper == null || string.IsNullOrEmpty(_currentWallpaper.FullPhotoUrl))
            {
                await ShowErrorDialogAsync("Failed to set wallpaper. No wallpaper image available.");
                return;
            }

            try
            {
                // Show loading dialog
                var loadingDialog = new ContentDialog
                {
                    Title = "Setting Wallpaper",
                    Content = "Please wait while we set your wallpaper...",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                // Show the dialog
                var dialogTask = loadingDialog.ShowAsync();

                // Start the background task to set wallpaper
                bool success = false;
                try
                {
                    // Get Pictures folder
                    var picturesFolder = KnownFolders.PicturesLibrary;
                    var wallpapersFolder = await picturesFolder.CreateFolderAsync("WallYouNeed", CreationCollisionOption.OpenIfExists);
                    
                    // Download the image
                    var imageBytes = await _httpClient.GetByteArrayAsync(_currentWallpaper.FullPhotoUrl);

                    // Create a file in Pictures folder
                    var wallpaperFile = await wallpapersFolder.CreateFileAsync(
                        $"wallpaper-{_currentWallpaper.Id}.jpg",
                        CreationCollisionOption.ReplaceExisting);

                    // Write the image to the file
                    using (var stream = await wallpaperFile.OpenStreamForWriteAsync())
                    {
                        await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                    }

                    // Try first method (WinRT API)
                    try 
                    {
                        success = await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(wallpaperFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WinRT API failed: {ex.Message}");
                    }

                    // If WinRT API fails, try P/Invoke method
                    if (!success)
                    {
                        success = WallpaperHelper.SetWallpaper(wallpaperFile.Path);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error setting wallpaper: {ex.Message}");
                }

                // Hide the loading dialog
                loadingDialog.Hide();

                // Show result to user
                if (success)
                {
                    await ShowSuccessDialogAsync("Wallpaper set successfully!");
                }
                else
                {
                    await ShowErrorDialogAsync("Failed to set wallpaper. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SetAsWallpaperButton_Click: {ex.Message}");
                await ShowErrorDialogAsync($"An error occurred: {ex.Message}");
            }
        }

        private async void ViewOnWebButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWallpaper == null || string.IsNullOrEmpty(_currentWallpaper.SourceUrl))
            {
                // Show error message
                await ShowErrorDialogAsync("Cannot view on web: Source URL is missing");
                return;
            }

            try
            {
                // Launch the default browser with the wallpaper's source URL
                Uri sourceUri = new Uri(_currentWallpaper.SourceUrl);
                bool success = await Windows.System.Launcher.LaunchUriAsync(sourceUri);
                
                if (!success)
                {
                    await ShowErrorDialogAsync("Failed to open browser");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening source URL: {ex.Message}");
                await ShowErrorDialogAsync("Error opening source URL");
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWallpaper == null || string.IsNullOrEmpty(_currentWallpaper.FullPhotoUrl))
            {
                await ShowErrorDialogAsync("Failed to download. No wallpaper image available.");
                return;
            }

            try
            {
                // Show progress dialog
                var progressDialog = new ContentDialog
                {
                    Title = "Downloading Wallpaper",
                    Content = "Please wait while your wallpaper downloads...",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };
                
                // Show the dialog
                var dialogTask = progressDialog.ShowAsync();
                
                try
                {
                    // Get Downloads folder using StorageFolder.GetFolderFromPathAsync
                    string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
                    var downloadsFolder = await StorageFolder.GetFolderFromPathAsync(downloadsPath);
                    
                    // Create a subfolder for our app if it doesn't exist
                    var appFolder = await downloadsFolder.CreateFolderAsync("WallYouNeed", CreationCollisionOption.OpenIfExists);
                    
                    // Create a unique filename based on wallpaper title and ID
                    string safeFileName = _currentWallpaper.Title.Replace(" ", "_");
                    safeFileName = string.Join("_", safeFileName.Split(IOPath.GetInvalidFileNameChars()));
                    var fileName = $"{safeFileName}_{_currentWallpaper.Id}.jpg";
                    
                    // Create the file in the downloads folder
                    var downloadFile = await appFolder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
                    
                    // Download directly to the file
                    var imageBytes = await _httpClient.GetByteArrayAsync(_currentWallpaper.FullPhotoUrl);
                    using (var stream = await downloadFile.OpenStreamForWriteAsync())
                    {
                        await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                    }
                    
                    // Hide progress dialog
                    progressDialog.Hide();
                    
                    // Show success message with the path
                    await ShowSuccessDialogAsync($"Wallpaper downloaded to:\n{downloadFile.Path}");
                }
                catch (Exception ex)
                {
                    // Hide progress dialog
                    progressDialog.Hide();
                    
                    Debug.WriteLine($"Error downloading file: {ex.Message}");
                    await ShowErrorDialogAsync($"Error downloading wallpaper: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DownloadButton_Click: {ex.Message}");
                await ShowErrorDialogAsync($"An error occurred: {ex.Message}");
            }
        }

        // Helper to try to get current window via reflection
        private Window GetCurrentWindowViaReflection()
        {
            try
            {
                // Try to get the current window
                var app = Application.Current;
                if (app != null)
                {
                    // Try via m_window field (private in App class)
                    var windowField = app.GetType().GetField("m_window", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.NonPublic);
                    
                    if (windowField != null)
                    {
                        return windowField.GetValue(app) as Window;
                    }
                    
                    // Alternatively try MainWindow property
                    var mainWindowProperty = app.GetType().GetProperty("MainWindow");
                    if (mainWindowProperty != null)
                    {
                        return mainWindowProperty.GetValue(app) as Window;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting window via reflection: {ex.Message}");
            }
            
            return null;
        }

        private async Task ShowSuccessDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowErrorDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}