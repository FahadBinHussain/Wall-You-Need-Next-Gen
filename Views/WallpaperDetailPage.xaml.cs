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
using Microsoft.UI.Xaml.Shapes; // for Ellipse
using Microsoft.UI.Xaml.Media; // for ImageBrush and Stretch
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using System.IO;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using System.Collections.Generic;
using Path = Microsoft.UI.Xaml.Shapes.Path; // Resolve ambiguous reference
using System.Linq;
using System.Reflection; // For reflection functionality

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
                    
                    // Extract publisher data
                    if (document.RootElement.TryGetProperty("WallpaperPublisher", out JsonElement publisherElement))
                    {
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
                        var wolfPath = FindName("PublisherProfileIcon") as Path;
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
                    // Get temporary folder
                    var tempFolder = ApplicationData.Current.TemporaryFolder;
                    
                    // Download the image
                    var imageBytes = await _httpClient.GetByteArrayAsync(_currentWallpaper.FullPhotoUrl);

                    // Create a temporary file
                    var tempFile = await tempFolder.CreateFileAsync(
                        $"wallpaper-{_currentWallpaper.Id}.jpg",
                        CreationCollisionOption.ReplaceExisting);

                    // Write the image to the file
                    using (var stream = await tempFile.OpenStreamForWriteAsync())
                    {
                        await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                    }

                    // Set as wallpaper using WinRT API
                    success = await Windows.System.UserProfile.UserProfilePersonalizationSettings
                        .Current.TrySetWallpaperImageAsync(tempFile);
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

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWallpaper == null || string.IsNullOrEmpty(_currentWallpaper.FullPhotoUrl))
            {
                await ShowErrorDialogAsync("Failed to download. No wallpaper image available.");
                return;
            }

            try
            {
                // Show information dialog
                var dialog = new ContentDialog
                {
                    Title = "Download Wallpaper",
                    Content = "Would you like to save this wallpaper to your device?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    // First download to temp folder
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
                        
                        // Get temp folder for initial download
                        var tempFolder = ApplicationData.Current.TemporaryFolder;
                        var tempFileName = $"wallpaper-{_currentWallpaper.Id}.jpg";
                        var tempFile = await tempFolder.CreateFileAsync(tempFileName, CreationCollisionOption.ReplaceExisting);
                        
                        // Download to temp file
                        var imageBytes = await _httpClient.GetByteArrayAsync(_currentWallpaper.FullPhotoUrl);
                        using (var stream = await tempFile.OpenStreamForWriteAsync())
                        {
                            await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                        }
                        
                        // Hide progress dialog
                        progressDialog.Hide();
                        
                        // Now let user pick where to save it
                        var savePicker = new FileSavePicker();
                        savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        savePicker.FileTypeChoices.Add("JPEG Image", new List<string>() { ".jpg" });
                        savePicker.SuggestedFileName = $"{_currentWallpaper.Title.Replace(" ", "_")}.jpg";
                        
                        // Try to get window handle - this is a WinUI 3 issue that requires reflection
                        try
                        {
                            // Get a window handle via reflection
                            var window = GetCurrentWindowViaReflection();
                            if (window != null)
                            {
                                var hwnd = WindowNative.GetWindowHandle(window);
                                InitializeWithWindow.Initialize(savePicker, hwnd);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed to initialize file picker: {ex.Message}");
                            await ShowErrorDialogAsync("Cannot open file picker. Please try again later.");
                            return;
                        }
                        
                        StorageFile file = await savePicker.PickSaveFileAsync();
                        if (file != null)
                        {
                            // Copy the temp file to the selected location
                            await tempFile.CopyAndReplaceAsync(file);
                            await ShowSuccessDialogAsync($"Wallpaper saved successfully!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error downloading file: {ex.Message}");
                        await ShowErrorDialogAsync($"Error downloading wallpaper: {ex.Message}");
                    }
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