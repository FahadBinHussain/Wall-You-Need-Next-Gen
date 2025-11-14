using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Aura.Models;

namespace Aura.Services
{
    public class SlideshowService
    {
        private static SlideshowService? _instance;
        public static SlideshowService Instance => _instance ??= new SlideshowService();

        private readonly AlphaCodersService _alphaCodersService;
        private readonly WallpaperService _wallpaperService;

        private DispatcherQueueTimer? _desktopTimer;
        private DispatcherQueueTimer? _lockScreenTimer;
        private List<WallpaperItem> _desktopWallpapers = new();
        private List<WallpaperItem> _lockScreenWallpapers = new();
        private int _desktopCurrentIndex = 0;
        private int _lockScreenCurrentIndex = 0;
        private readonly HttpClient _httpClient = new();
        
        // Track current platform for desktop and lockscreen
        private string _desktopPlatform = "";
        private string _lockScreenPlatform = "";
        
        // Current wallpaper URLs for display
        private string _currentDesktopWallpaperUrl = "";
        private string _currentLockScreenWallpaperUrl = "";
        
        // Events for wallpaper changes
        public event EventHandler<string>? DesktopWallpaperChanged;
        public event EventHandler<string>? LockScreenWallpaperChanged;

        // Windows API for setting desktop wallpaper
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;

        private SlideshowService()
        {
            _alphaCodersService = new AlphaCodersService();
            _wallpaperService = new WallpaperService();
        }

        private void LogInfo(string message)
        {
            try
            {
                ((App)Application.Current).LogInfo($"[SlideshowService] {message}");
            }
            catch
            {
                Debug.WriteLine($"[SlideshowService] {message}");
            }
        }

        public async Task StartDesktopSlideshow(string platform, string category, TimeSpan interval, DispatcherQueue dispatcherQueue)
        {
            try
            {
                LogInfo($"Starting desktop slideshow: {platform} - {category}, Interval: {interval}");

                // Stop existing timer if any
                StopDesktopSlideshow();

                // Fetch wallpapers
                await LoadWallpapersForDesktop(platform, category);

                if (_desktopWallpapers.Count == 0)
                {
                    LogInfo("ERROR: No wallpapers loaded for desktop slideshow");
                    return;
                }

                LogInfo($"Loaded {_desktopWallpapers.Count} wallpapers");

                // Set first wallpaper immediately
                await SetDesktopWallpaper(_desktopWallpapers[0]);

                // Create and start timer
                _desktopTimer = dispatcherQueue.CreateTimer();
                _desktopTimer.Interval = interval;
                _desktopTimer.Tick += async (sender, e) =>
                {
                    _desktopCurrentIndex = (_desktopCurrentIndex + 1) % _desktopWallpapers.Count;
                    await SetDesktopWallpaper(_desktopWallpapers[_desktopCurrentIndex]);
                };
                _desktopTimer.Start();

                LogInfo($"Desktop slideshow started successfully with {_desktopWallpapers.Count} wallpapers");
            }
            catch (Exception ex)
            {
                LogInfo($"ERROR starting desktop slideshow: {ex.Message}");
                LogInfo($"Stack trace: {ex.StackTrace}");
            }
        }

        public async Task StartLockScreenSlideshow(string platform, string category, TimeSpan interval, DispatcherQueue dispatcherQueue)
        {
            LogInfo($"Starting lock screen slideshow: {platform} - {category}, Interval: {interval}");

            // Stop existing timer if any
            StopLockScreenSlideshow();

            // Fetch wallpapers
            await LoadWallpapersForLockScreen(platform, category);

            if (_lockScreenWallpapers.Count == 0)
            {
                LogInfo("No wallpapers loaded for lock screen slideshow");
                return;
            }

            // Set first wallpaper immediately
            await SetLockScreenWallpaper(_lockScreenWallpapers[0]);

            // Create and start timer
            _lockScreenTimer = dispatcherQueue.CreateTimer();
            _lockScreenTimer.Interval = interval;
            _lockScreenTimer.Tick += async (sender, e) =>
            {
                _lockScreenCurrentIndex = (_lockScreenCurrentIndex + 1) % _lockScreenWallpapers.Count;
                await SetLockScreenWallpaper(_lockScreenWallpapers[_lockScreenCurrentIndex]);
            };
            _lockScreenTimer.Start();

            LogInfo($"Lock screen slideshow started with {_lockScreenWallpapers.Count} wallpapers");
        }

        public void StopDesktopSlideshow()
        {
            if (_desktopTimer != null)
            {
                _desktopTimer.Stop();
                _desktopTimer = null;
                LogInfo("Desktop slideshow stopped");
            }
        }

        public void StopLockScreenSlideshow()
        {
            if (_lockScreenTimer != null)
            {
                _lockScreenTimer.Stop();
                _lockScreenTimer = null;
                LogInfo("Lock screen slideshow stopped");
            }
        }

        public async Task NextDesktopWallpaper()
        {
            if (_desktopWallpapers.Count > 0)
            {
                _desktopCurrentIndex = (_desktopCurrentIndex + 1) % _desktopWallpapers.Count;
                await SetDesktopWallpaper(_desktopWallpapers[_desktopCurrentIndex]);
            }
        }

        public async Task NextLockScreenWallpaper()
        {
            if (_lockScreenWallpapers.Count > 0)
            {
                _lockScreenCurrentIndex = (_lockScreenCurrentIndex + 1) % _lockScreenWallpapers.Count;
                await SetLockScreenWallpaper(_lockScreenWallpapers[_lockScreenCurrentIndex]);
            }
        }

        private async Task LoadWallpapersForDesktop(string platform, string category)
        {
            try
            {
                _desktopWallpapers.Clear();
                _desktopCurrentIndex = 0;
                _desktopPlatform = platform; // Store the platform

                LogInfo($"Loading wallpapers - Platform: {platform}, Category: {category}");

                if (platform == "AlphaCoders")
                {
                    // Get wallpapers from AlphaCoders service
                    string categoryKey = category switch
                    {
                        "4K Wallpapers" => "4k",
                        "Harvest Wallpapers" => "harvest",
                        "Rain Wallpapers" => "rain",
                        _ => "4k"
                    };

                    LogInfo($"Fetching from AlphaCoders with category key: {categoryKey}");
                    var wallpapers = await _alphaCodersService.GetWallpapersByCategoryAsync(categoryKey, 1, 50);
                    _desktopWallpapers.AddRange(wallpapers);
                    LogInfo($"Loaded {wallpapers.Count} wallpapers from AlphaCoders");
                }
                else // Backiee
                {
                    LogInfo($"Fetching from Backiee API");
                    
                    // Fetch from Backiee API just like LatestWallpapersPage does
                    string apiUrl = "https://backiee.com/api/wallpaper/list.php?action=paging_list&list_type=latest&page=0&page_size=50&category=all&is_ai=all&sort_by=popularity&4k=false&5k=false&8k=false&status=active&args=";
                    
                    HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonContent = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                        {
                            foreach (JsonElement wallpaperElement in doc.RootElement.EnumerateArray())
                            {
                                string id = wallpaperElement.GetProperty("ID").GetString();
                                string title = wallpaperElement.GetProperty("Title").GetString();
                                string fullPhotoUrl = wallpaperElement.GetProperty("FullPhotoUrl").GetString();
                                
                                _desktopWallpapers.Add(new WallpaperItem
                                {
                                    Id = id,
                                    Title = title,
                                    FullPhotoUrl = fullPhotoUrl
                                });
                            }
                        }
                        LogInfo($"Loaded {_desktopWallpapers.Count} wallpapers from Backiee API");
                    }
                    else
                    {
                        LogInfo($"ERROR: Backiee API returned status {response.StatusCode}");
                    }
                }

                LogInfo($"Total wallpapers in collection: {_desktopWallpapers.Count}");
            }
            catch (Exception ex)
            {
                LogInfo($"ERROR loading wallpapers: {ex.Message}");
                LogInfo($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task LoadWallpapersForLockScreen(string platform, string category)
        {
            _lockScreenWallpapers.Clear();
            _lockScreenCurrentIndex = 0;
            _lockScreenPlatform = platform; // Store the platform

            if (platform == "AlphaCoders")
            {
                // Get wallpapers from AlphaCoders service
                string categoryKey = category switch
                {
                    "4K Wallpapers" => "4k",
                    "Harvest Wallpapers" => "harvest",
                    "Rain Wallpapers" => "rain",
                    _ => "4k"
                };

                var wallpapers = await _alphaCodersService.GetWallpapersByCategoryAsync(categoryKey, 1, 50);
                _lockScreenWallpapers.AddRange(wallpapers);
            }
            else // Backiee
            {
                // Fetch from Backiee API just like LatestWallpapersPage does
                string apiUrl = "https://backiee.com/api/wallpaper/list.php?action=paging_list&list_type=latest&page=0&page_size=50&category=all&is_ai=all&sort_by=popularity&4k=false&5k=false&8k=false&status=active&args=";
                
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                    {
                        foreach (JsonElement wallpaperElement in doc.RootElement.EnumerateArray())
                        {
                            string id = wallpaperElement.GetProperty("ID").GetString();
                            string title = wallpaperElement.GetProperty("Title").GetString();
                            string fullPhotoUrl = wallpaperElement.GetProperty("FullPhotoUrl").GetString();
                            
                            _lockScreenWallpapers.Add(new WallpaperItem
                            {
                                Id = id,
                                Title = title,
                                FullPhotoUrl = fullPhotoUrl
                            });
                        }
                    }
                }
            }
        }

        private async Task SetDesktopWallpaper(WallpaperItem wallpaper)
        {
            try
            {
                // Use platform-specific logic
                if (_desktopPlatform == "AlphaCoders")
                {
                    await SetDesktopWallpaper_AlphaCoders(wallpaper);
                }
                else // Backiee
                {
                    await SetDesktopWallpaper_Backiee(wallpaper);
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Error setting desktop wallpaper: {ex.Message}");
            }
        }

        private async Task SetDesktopWallpaper_Backiee(WallpaperItem wallpaper)
        {
            try
            {
                string imageUrl = wallpaper.FullPhotoUrl;
                if (string.IsNullOrEmpty(imageUrl))
                {
                    LogInfo($"No image URL available for wallpaper: {wallpaper.Title}");
                    return;
                }

                // Get Pictures folder
                var picturesFolder = Windows.Storage.KnownFolders.PicturesLibrary;
                var wallpapersFolder = await picturesFolder.CreateFolderAsync("Aura", Windows.Storage.CreationCollisionOption.OpenIfExists);

                // Download the image
                byte[] imageBytes;
                if (imageUrl.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle local app resources
                    string localPath = imageUrl.Replace("ms-appx:///", "").Replace("/", "\\");
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    string fullLocalPath = Path.Combine(appPath, localPath);
                    
                    LogInfo($"Attempting to load local file: {fullLocalPath}");
                    
                    if (File.Exists(fullLocalPath))
                    {
                        imageBytes = await File.ReadAllBytesAsync(fullLocalPath);
                        LogInfo($"Successfully loaded local file");
                    }
                    else
                    {
                        LogInfo($"Local file not found: {fullLocalPath}");
                        return;
                    }
                }
                else
                {
                    // Download from HTTP URL
                    LogInfo($"Downloading from URL: {imageUrl}");
                    imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                }

                // Create a file in Pictures folder
                var wallpaperFile = await wallpapersFolder.CreateFileAsync(
                    $"wallpaper-{wallpaper.Id}.jpg",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);

                // Write the image to the file
                using (var stream = await wallpaperFile.OpenStreamForWriteAsync())
                {
                    await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }

                LogInfo($"Wallpaper saved to: {wallpaperFile.Path}");

                // Try to set the wallpaper using WinRT API
                bool success = false;
                var userProfilePersonalizationSettings = Windows.System.UserProfile.UserProfilePersonalizationSettings.Current;

                try
                {
                    success = await userProfilePersonalizationSettings.TrySetWallpaperImageAsync(wallpaperFile);
                    LogInfo($"WinRT API result for desktop wallpaper: {success}");
                }
                catch (Exception ex)
                {
                    LogInfo($"WinRT API failed for desktop wallpaper: {ex.Message}");
                }

                // If WinRT API fails, try WallpaperHelper as fallback
                if (!success)
                {
                    try
                    {
                        LogInfo("WinRT API failed, trying SystemParametersInfo fallback...");
                        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperFile.Path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        LogInfo($"SystemParametersInfo fallback also failed: {ex.Message}");
                    }
                }

                if (success)
                {
                    LogInfo($"Desktop wallpaper set to: {wallpaper.Title}");
                    
                    // Store the current wallpaper URL and raise event
                    _currentDesktopWallpaperUrl = imageUrl;
                    DesktopWallpaperChanged?.Invoke(this, imageUrl);
                }
                else
                {
                    LogInfo($"Failed to set desktop wallpaper: {wallpaper.Title}");
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Error setting desktop wallpaper: {ex.Message}");
                LogInfo($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task SetDesktopWallpaper_AlphaCoders(WallpaperItem wallpaper)
        {
            try
            {
                if (string.IsNullOrEmpty(wallpaper.ImageUrl))
                {
                    LogInfo($"No image URL available for AlphaCoders wallpaper: {wallpaper.Title}");
                    return;
                }

                // Get Pictures folder
                var picturesFolder = Windows.Storage.KnownFolders.PicturesLibrary;
                var wallpapersFolder = await picturesFolder.CreateFolderAsync("Aura", Windows.Storage.CreationCollisionOption.OpenIfExists);

                // Get the big thumb URL to extract extension
                var scraperService = new AlphaCodersScraperService();
                var bigThumbUrl = await scraperService.GetBigImageUrlForWallpaperAsync(wallpaper.Id, wallpaper.ImageUrl);

                if (string.IsNullOrEmpty(bigThumbUrl))
                {
                    LogInfo($"Could not get big image URL for AlphaCoders wallpaper: {wallpaper.Title}");
                    return;
                }

                // Extract extension from big thumb URL
                var bigThumbUri = new Uri(bigThumbUrl);
                var bigThumbPath = bigThumbUri.AbsolutePath;
                var extension = System.IO.Path.GetExtension(bigThumbPath).TrimStart('.');

                // Build original URL with same extension
                var imageId = wallpaper.Id;
                var uri = new Uri(wallpaper.ImageUrl);
                var domainParts = uri.Host.Split('.');
                var domainShort = domainParts[0];

                var originalUrl = $"https://initiate.alphacoders.com/download/{domainShort}/{imageId}/{extension}";

                LogInfo($"Downloading AlphaCoders wallpaper from: {originalUrl}");

                byte[] imageBytes;
                try
                {
                    imageBytes = await _httpClient.GetByteArrayAsync(originalUrl);
                    LogInfo($"Successfully downloaded AlphaCoders wallpaper");
                }
                catch (Exception ex)
                {
                    LogInfo($"Failed to download from {originalUrl}: {ex.Message}");
                    return;
                }

                // Create a file in Pictures folder with a unique name based on timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileExtension = !string.IsNullOrEmpty(extension) ? extension : "jpg";

                var wallpaperFile = await wallpapersFolder.CreateFileAsync(
                    $"wallpaper-{wallpaper.Id}-{timestamp}.{fileExtension}",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);

                // Write the image to the file
                using (var stream = await wallpaperFile.OpenStreamForWriteAsync())
                {
                    await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }

                LogInfo($"AlphaCoders wallpaper saved to: {wallpaperFile.Path}");

                // Try to set the wallpaper using WinRT API
                bool success = false;
                var userProfilePersonalizationSettings = Windows.System.UserProfile.UserProfilePersonalizationSettings.Current;

                try
                {
                    success = await userProfilePersonalizationSettings.TrySetWallpaperImageAsync(wallpaperFile);
                    LogInfo($"WinRT API result for AlphaCoders desktop wallpaper: {success}");
                }
                catch (Exception ex)
                {
                    LogInfo($"WinRT API failed for AlphaCoders desktop wallpaper: {ex.Message}");
                }

                // If WinRT API fails, try SystemParametersInfo as fallback
                if (!success)
                {
                    try
                    {
                        LogInfo("WinRT API failed, trying SystemParametersInfo fallback for AlphaCoders...");
                        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperFile.Path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        LogInfo($"SystemParametersInfo fallback also failed for AlphaCoders: {ex.Message}");
                    }
                }

                if (success)
                {
                    LogInfo($"AlphaCoders desktop wallpaper set to: {wallpaper.Title}");
                    
                    // Store the current wallpaper URL and raise event
                    _currentDesktopWallpaperUrl = originalUrl;
                    DesktopWallpaperChanged?.Invoke(this, originalUrl);
                }
                else
                {
                    LogInfo($"Failed to set AlphaCoders desktop wallpaper: {wallpaper.Title}");
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Error setting AlphaCoders desktop wallpaper: {ex.Message}");
                LogInfo($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task SetLockScreenWallpaper(WallpaperItem wallpaper)
        {
            try
            {
                // Use platform-specific logic
                if (_lockScreenPlatform == "AlphaCoders")
                {
                    await SetLockScreenWallpaper_AlphaCoders(wallpaper);
                }
                else // Backiee
                {
                    await SetLockScreenWallpaper_Backiee(wallpaper);
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Error setting lock screen wallpaper: {ex.Message}");
            }
        }

        private async Task SetLockScreenWallpaper_Backiee(WallpaperItem wallpaper)
        {
            try
            {
                string imageUrl = wallpaper.FullPhotoUrl;
                if (string.IsNullOrEmpty(imageUrl))
                {
                    LogInfo($"No image URL available for wallpaper: {wallpaper.Title}");
                    return;
                }

                // Get Pictures folder
                var picturesFolder = Windows.Storage.KnownFolders.PicturesLibrary;
                var wallpapersFolder = await picturesFolder.CreateFolderAsync("Aura", Windows.Storage.CreationCollisionOption.OpenIfExists);

                // Download the image
                byte[] imageBytes;
                if (imageUrl.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
                {
                    // Handle local app resources
                    string localPath = imageUrl.Replace("ms-appx:///", "").Replace("/", "\\");
                    string appPath = AppDomain.CurrentDomain.BaseDirectory;
                    string fullLocalPath = Path.Combine(appPath, localPath);
                    
                    LogInfo($"Attempting to load local file: {fullLocalPath}");
                    
                    if (File.Exists(fullLocalPath))
                    {
                        imageBytes = await File.ReadAllBytesAsync(fullLocalPath);
                        LogInfo($"Successfully loaded local file");
                    }
                    else
                    {
                        LogInfo($"Local file not found: {fullLocalPath}");
                        return;
                    }
                }
                else
                {
                    // Download from HTTP URL
                    LogInfo($"Downloading from URL: {imageUrl}");
                    imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                }

                // Create a file in Pictures folder
                var wallpaperFile = await wallpapersFolder.CreateFileAsync(
                    $"lockscreen-{wallpaper.Id}.jpg",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);

                // Write the image to the file
                using (var stream = await wallpaperFile.OpenStreamForWriteAsync())
                {
                    await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }

                LogInfo($"Lock screen image saved to: {wallpaperFile.Path}");

                // Use registry method (same as WallpaperDetailPage)
                bool success = false;
                const string PERSONALIZE_REG_KEY = @"Software\Microsoft\Windows\CurrentVersion\PersonalizationCSP";
                const string LOCKSCREEN_PATH_REG_VALUE = "LockScreenImagePath";
                const string LOCKSCREEN_STATUS_REG_VALUE = "LockScreenImageStatus";
                const string LOCKSCREEN_URL_REG_VALUE = "LockScreenImageUrl";

                // Method: Using Registry for lock screen with LocalMachine
                try
                {
                    LogInfo("Trying LocalMachine registry for lock screen...");
                    using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(PERSONALIZE_REG_KEY, true))
                    {
                        if (key == null)
                        {
                            // Try to create the key if it doesn't exist
                            using (var newKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(PERSONALIZE_REG_KEY, true))
                            {
                                if (newKey != null)
                                {
                                    newKey.SetValue(LOCKSCREEN_PATH_REG_VALUE, wallpaperFile.Path);
                                    newKey.SetValue(LOCKSCREEN_STATUS_REG_VALUE, 1);
                                    newKey.SetValue(LOCKSCREEN_URL_REG_VALUE, wallpaperFile.Path);
                                    success = true;
                                    LogInfo("Registry method for lock screen succeeded (created key in LocalMachine)");
                                }
                            }
                        }
                        else
                        {
                            // Key exists, set the values
                            key.SetValue(LOCKSCREEN_PATH_REG_VALUE, wallpaperFile.Path);
                            key.SetValue(LOCKSCREEN_STATUS_REG_VALUE, 1);
                            key.SetValue(LOCKSCREEN_URL_REG_VALUE, wallpaperFile.Path);
                            success = true;
                            LogInfo("Registry method for lock screen succeeded (updated key in LocalMachine)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogInfo($"Registry method for lock screen failed: {ex.Message}");
                }

                // If LocalMachine failed, try with CurrentUser as fallback
                if (!success)
                {
                    try
                    {
                        LogInfo("Trying CurrentUser registry for lock screen...");
                        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(PERSONALIZE_REG_KEY, true))
                        {
                            if (key == null)
                            {
                                // Try to create the key if it doesn't exist
                                using (var newKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(PERSONALIZE_REG_KEY, true))
                                {
                                    if (newKey != null)
                                    {
                                        newKey.SetValue(LOCKSCREEN_PATH_REG_VALUE, wallpaperFile.Path);
                                        newKey.SetValue(LOCKSCREEN_STATUS_REG_VALUE, 1);
                                        newKey.SetValue(LOCKSCREEN_URL_REG_VALUE, wallpaperFile.Path);
                                        success = true;
                                        LogInfo("Registry method for lock screen succeeded (created key in CurrentUser)");
                                    }
                                }
                            }
                            else
                            {
                                // Key exists, set the values
                                key.SetValue(LOCKSCREEN_PATH_REG_VALUE, wallpaperFile.Path);
                                key.SetValue(LOCKSCREEN_STATUS_REG_VALUE, 1);
                                key.SetValue(LOCKSCREEN_URL_REG_VALUE, wallpaperFile.Path);
                                success = true;
                                LogInfo("Registry method for lock screen succeeded (updated key in CurrentUser)");
                            }
                        }
                    }
                    catch (Exception innerEx)
                    {
                        LogInfo($"CurrentUser registry method for lock screen also failed: {innerEx.Message}");
                    }
                }

                if (success)
                {
                    LogInfo($"Lock screen set to: {wallpaper.Title}");
                    
                    // Store the current wallpaper URL and raise event
                    _currentLockScreenWallpaperUrl = imageUrl;
                    LockScreenWallpaperChanged?.Invoke(this, imageUrl);
                }
                else
                {
                    LogInfo($"Failed to set lock screen: {wallpaper.Title}");
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Error setting lock screen wallpaper: {ex.Message}");
                LogInfo($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task SetLockScreenWallpaper_AlphaCoders(WallpaperItem wallpaper)
        {
            try
            {
                if (string.IsNullOrEmpty(wallpaper.ImageUrl))
                {
                    LogInfo($"No image URL available for AlphaCoders wallpaper: {wallpaper.Title}");
                    return;
                }

                // Get Pictures folder
                var picturesFolder = Windows.Storage.KnownFolders.PicturesLibrary;
                var wallpapersFolder = await picturesFolder.CreateFolderAsync("Aura", Windows.Storage.CreationCollisionOption.OpenIfExists);

                // Get the big thumb URL to extract extension
                var scraperService = new AlphaCodersScraperService();
                var bigThumbUrl = await scraperService.GetBigImageUrlForWallpaperAsync(wallpaper.Id, wallpaper.ImageUrl);

                if (string.IsNullOrEmpty(bigThumbUrl))
                {
                    LogInfo($"Could not get big image URL for AlphaCoders wallpaper: {wallpaper.Title}");
                    return;
                }

                // Extract extension from big thumb URL
                var bigThumbUri = new Uri(bigThumbUrl);
                var bigThumbPath = bigThumbUri.AbsolutePath;
                var extension = System.IO.Path.GetExtension(bigThumbPath).TrimStart('.');

                // Build original URL with same extension
                var imageId = wallpaper.Id;
                var uri = new Uri(wallpaper.ImageUrl);
                var domainParts = uri.Host.Split('.');
                var domainShort = domainParts[0];

                var originalUrl = $"https://initiate.alphacoders.com/download/{domainShort}/{imageId}/{extension}";

                LogInfo($"Downloading AlphaCoders wallpaper from: {originalUrl}");

                byte[] imageBytes;
                try
                {
                    imageBytes = await _httpClient.GetByteArrayAsync(originalUrl);
                    LogInfo($"Successfully downloaded AlphaCoders wallpaper for lock screen");
                }
                catch (Exception ex)
                {
                    LogInfo($"Failed to download from {originalUrl}: {ex.Message}");
                    return;
                }

                // Create a file in Pictures folder with a unique name based on timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileExtension = !string.IsNullOrEmpty(extension) ? extension : "jpg";

                var wallpaperFile = await wallpapersFolder.CreateFileAsync(
                    $"lockscreen-{wallpaper.Id}-{timestamp}.{fileExtension}",
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);

                // Write the image to the file
                using (var stream = await wallpaperFile.OpenStreamForWriteAsync())
                {
                    await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                }

                LogInfo($"AlphaCoders lock screen wallpaper saved to: {wallpaperFile.Path}");

                // Try to set the lock screen using WinRT API
                bool success = false;
                var userProfilePersonalizationSettings = Windows.System.UserProfile.UserProfilePersonalizationSettings.Current;

                try
                {
                    success = await userProfilePersonalizationSettings.TrySetLockScreenImageAsync(wallpaperFile);
                    LogInfo($"WinRT API result for AlphaCoders lock screen: {success}");
                }
                catch (Exception ex)
                {
                    LogInfo($"WinRT API failed for AlphaCoders lock screen: {ex.Message}");
                }

                if (success)
                {
                    LogInfo($"AlphaCoders lock screen set to: {wallpaper.Title}");
                    
                    // Store the current wallpaper URL and raise event
                    _currentLockScreenWallpaperUrl = originalUrl;
                    LockScreenWallpaperChanged?.Invoke(this, originalUrl);
                }
                else
                {
                    LogInfo($"Failed to set AlphaCoders lock screen: {wallpaper.Title}");
                }
            }
            catch (Exception ex)
            {
                LogInfo($"Error setting AlphaCoders lock screen wallpaper: {ex.Message}");
                LogInfo($"Stack trace: {ex.StackTrace}");
            }
        }

        public static TimeSpan ParseInterval(string interval)
        {
            if (string.IsNullOrWhiteSpace(interval))
                return TimeSpan.FromHours(12); // Default

            var parts = interval.Trim().Split(' ');
            if (parts.Length < 2)
                return TimeSpan.FromHours(12); // Default

            if (!double.TryParse(parts[0], out double value))
                return TimeSpan.FromHours(12); // Default

            string unit = parts[1].Trim().ToLower();

            // Handle both singular and plural forms
            if (unit == "second" || unit == "seconds")
                return TimeSpan.FromSeconds(value);
            else if (unit == "minute" || unit == "minutes")
                return TimeSpan.FromMinutes(value);
            else if (unit == "hour" || unit == "hours")
                return TimeSpan.FromHours(value);
            else if (unit == "day" || unit == "days")
                return TimeSpan.FromDays(value);
            else
                return TimeSpan.FromHours(12); // Default
        }
        
        public string GetCurrentDesktopWallpaperUrl()
        {
            return _currentDesktopWallpaperUrl;
        }
        
        public string GetCurrentLockScreenWallpaperUrl()
        {
            return _currentLockScreenWallpaperUrl;
        }
        
        public WallpaperItem? GetCurrentDesktopWallpaperItem()
        {
            if (_desktopWallpapers.Count > 0 && _desktopCurrentIndex < _desktopWallpapers.Count)
            {
                return _desktopWallpapers[_desktopCurrentIndex];
            }
            return null;
        }
        
        public WallpaperItem? GetCurrentLockScreenWallpaperItem()
        {
            if (_lockScreenWallpapers.Count > 0 && _lockScreenCurrentIndex < _lockScreenWallpapers.Count)
            {
                return _lockScreenWallpapers[_lockScreenCurrentIndex];
            }
            return null;
        }
    }
}
