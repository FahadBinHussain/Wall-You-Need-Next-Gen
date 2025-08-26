using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Windows.Storage.Streams;

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
            if (string.IsNullOrEmpty(ImageUrl))
                return null;
                
            try
            {
                // For local images, create a BitmapImage directly
                if (ImageUrl.StartsWith("ms-appx:"))
                {
                    return new BitmapImage(new Uri(ImageUrl));
                }
                
                // For remote images, create a BitmapImage from the URL
                var bitmap = new BitmapImage(new Uri(ImageUrl));
                
                // Important: log the URL we're trying to load
                System.Diagnostics.Debug.WriteLine($"Loading image from URL: {ImageUrl}");
                
                // Add a small delay to ensure the image loads properly
                await Task.Delay(10);
                
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating image: {ex.Message}");
                return null;
            }
        }
        
        // Method to load the full image
        public async Task<BitmapImage> LoadFullImageAsync()
        {
            if (string.IsNullOrEmpty(FullPhotoUrl))
                return null;

            try
            {
                // For local images, create a BitmapImage directly
                if (FullPhotoUrl.StartsWith("ms-appx:"))
                {
                    return new BitmapImage(new Uri(FullPhotoUrl));
                }
                
                // For remote images, create a BitmapImage from the URL
                var bitmap = new BitmapImage(new Uri(FullPhotoUrl));
                
                // Important: log the URL we're trying to load
                System.Diagnostics.Debug.WriteLine($"Loading full image from URL: {FullPhotoUrl}");
                
                // Add a small delay to ensure the image loads properly
                await Task.Delay(10);
                
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading full image from URL {FullPhotoUrl}: {ex.Message}");
                return null;
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