using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Windows.Input;

namespace Wall_You_Need_Next_Gen.Models
{
    // Model for wallpaper items
    public class WallpaperItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
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