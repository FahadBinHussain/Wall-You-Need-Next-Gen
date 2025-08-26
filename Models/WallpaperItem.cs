using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime; // For AsStreamForWrite extension method
using System.IO; // For MemoryStream
using Windows.Storage.Streams; // For InMemoryRandomAccessStream

namespace Wall_You_Need_Next_Gen.Models
{
    // Model for wallpaper items
    public class WallpaperItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty; // URL for the thumbnail
        public string FullPhotoUrl { get; set; } = string.Empty; // URL for the full size image
        public string SourceUrl { get; set; } = string.Empty; // URL for the source webpage
        public BitmapImage ImageSource { get; set; } // The loaded image source for the thumbnail
        public string Resolution { get; set; } = string.Empty;

        // Properties for the tags
        public string QualityTag { get; set; } = string.Empty; // e.g., 4K, 8K, UltraHD
        public bool IsAI { get; set; }
        public string Likes { get; set; } = "0";
        public string Downloads { get; set; } = "0";

        public ICommand DownloadCommand { get; set; }

        // Get the appropriate logo path based on the quality tag
        public string QualityLogoPath
        {
            get
            {
                if (QualityTag?.ToUpper() == "4K") return "ms-appx:///Assets/4k_logo.png";
                if (QualityTag?.ToUpper() == "5K") return "ms-appx:///Assets/5k_logo.png";
                if (QualityTag?.ToUpper() == "8K") return "ms-appx:///Assets/8k_logo.png";
                // Add other quality types if needed
                return string.Empty;
            }
        }

        // Async method to load the actual image when needed
        public async Task<BitmapImage> LoadImageAsync()
        {
            // Important: log the URL we're trying to load
            System.Diagnostics.Debug.WriteLine($"LoadImageAsync: Starting to load image from URL: {ImageUrl}");

            // For remote images, use HttpClient to download the image data first
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                // Add browser-like headers
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://wall.alphacoders.com/");

                System.Diagnostics.Debug.WriteLine($"LoadImageAsync: Making HTTP request to {ImageUrl}");

                // Download the image data
                var imageBytes = await httpClient.GetByteArrayAsync(ImageUrl);

                System.Diagnostics.Debug.WriteLine($"LoadImageAsync: Downloaded {imageBytes.Length} bytes for {ImageUrl}");

                // Create a memory stream from the downloaded data
                using (var memStream = new System.IO.MemoryStream(imageBytes))
                {
                    System.Diagnostics.Debug.WriteLine($"LoadImageAsync: Creating BitmapImage for {ImageUrl}");

                    // Create a bitmap image
                    var bitmap = new BitmapImage();
                    bitmap.DecodePixelWidth = 500; // Adjust based on your UI needs

                    // Convert MemoryStream to IRandomAccessStream
                    var randomAccessStream = new InMemoryRandomAccessStream();
                    var outputStream = randomAccessStream.GetOutputStreamAt(0);
                    await memStream.CopyToAsync(outputStream.AsStreamForWrite());
                    await outputStream.FlushAsync();

                    System.Diagnostics.Debug.WriteLine($"LoadImageAsync: Setting bitmap source for {ImageUrl}");

                    // Set the source from the random access stream
                    await bitmap.SetSourceAsync(randomAccessStream);

                    System.Diagnostics.Debug.WriteLine($"LoadImageAsync: Successfully created bitmap for {ImageUrl}");

                    return bitmap;
                }
            }
        }

        // Method to load the full image
        public async Task<BitmapImage> LoadFullImageAsync()
        {
            // Important: log the URL we're trying to load
            System.Diagnostics.Debug.WriteLine($"Loading full image from URL: {FullPhotoUrl}");

            // For remote images, use HttpClient to download the image data first
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                // Add browser-like headers
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                httpClient.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/*,*/*;q=0.8");
                httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                httpClient.DefaultRequestHeaders.Add("Referer", "https://wall.alphacoders.com/");

                // Download the image data
                var imageBytes = await httpClient.GetByteArrayAsync(FullPhotoUrl);

                // Create a memory stream from the downloaded data
                using (var memStream = new System.IO.MemoryStream(imageBytes))
                {
                    // Create a bitmap image
                    var bitmap = new BitmapImage();

                    // Convert MemoryStream to IRandomAccessStream
                    var randomAccessStream = new InMemoryRandomAccessStream();
                    var outputStream = randomAccessStream.GetOutputStreamAt(0);
                    await memStream.CopyToAsync(outputStream.AsStreamForWrite());
                    await outputStream.FlushAsync();

                    // Set the source from the random access stream
                    await bitmap.SetSourceAsync(randomAccessStream);

                    return bitmap;
                }
            }
        }

        public WallpaperItem()
        {
            // Initialize the download command
            DownloadCommand = new RelayCommand(_ =>
            {
                // This would download the wallpaper
                // Not implemented in this placeholder version
            });
        }
    }

    // Simple RelayCommand implementation for the DownloadCommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
