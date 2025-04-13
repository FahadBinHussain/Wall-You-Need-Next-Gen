using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Wall_You_Need_Next_Gen.Models
{
    // Model for wallpaper items
    public class WallpaperItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public BitmapImage ImageSource { get; set; }
        public string Resolution { get; set; }
        
        // Properties for the tags
        public string QualityTag { get; set; } // 4K, 5K, 8K
        public bool IsAI { get; set; }
        public string Likes { get; set; }
        public string Downloads { get; set; }
        
        public ICommand DownloadCommand { get; set; }
        
        // Get the appropriate logo path based on the quality tag
        public string QualityLogoPath
        {
            get
            {
                if (string.IsNullOrEmpty(QualityTag))
                    return null;
                
                // Convert the quality tag to lowercase and remove any spaces
                string normalizedTag = QualityTag.ToLowerInvariant().Replace(" ", "");
                
                // Map the normalized tag to the corresponding logo path
                if (normalizedTag.Contains("4k"))
                    return "ms-appx:///Assets/4k_logo.png";
                else if (normalizedTag.Contains("5k"))
                    return "ms-appx:///Assets/5k_logo.png";
                else if (normalizedTag.Contains("8k"))
                    return "ms-appx:///Assets/8k_logo.png";
                
                // Return null if no matching logo is found
                return null;
            }
        }
        
        // Async method to load the actual image when needed
        public async Task<BitmapImage> LoadImageAsync()
        {
            if (string.IsNullOrEmpty(ImageUrl))
                return null;
                
            try
            {
                // Create a simple BitmapImage - the simpler approach often works better
                var bitmap = new BitmapImage();
                
                // Set bitmap properties
                bitmap.CreateOptions = BitmapCreateOptions.None;
                
                // Important: log the URL we're trying to load
                System.Diagnostics.Debug.WriteLine($"Loading image from URL: {ImageUrl}");
                
                // Set URI source directly without the complex TaskCompletionSource
                bitmap.UriSource = new Uri(ImageUrl);
                
                // Use a simple timeout to give the image a chance to load
                await Task.Delay(100); // Small delay to allow the loading to start
                
                // Return the bitmap - the image will continue loading asynchronously
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating image: {ex.Message}");
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