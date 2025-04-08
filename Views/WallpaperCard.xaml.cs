using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Wall_You_Need_Next_Gen.Models;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class WallpaperCard : UserControl
    {
        private Wallpaper _wallpaper;

        public WallpaperCard()
        {
            this.InitializeComponent();
        }

        public void SetWallpaper(Wallpaper wallpaper)
        {
            _wallpaper = wallpaper;
            
            // Set image source
            try
            {
                WallpaperImage.Source = new BitmapImage(new System.Uri(wallpaper.ImagePath));
            }
            catch 
            {
                // Use placeholder if image loading fails
                WallpaperImage.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/placeholder-dark.png"));
            }
            
            // Set resolution text and show appropriate resolution icon
            ResolutionText.Text = wallpaper.Resolution;
            
            // Set resolution icon based on resolution
            if (wallpaper.Resolution.Contains("4K"))
            {
                ResolutionImage.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/4k_logo.png"));
            }
            else if (wallpaper.Resolution.Contains("5K"))
            {
                ResolutionImage.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/5k_logo.png"));
            }
            else if (wallpaper.Resolution.Contains("8K"))
            {
                ResolutionImage.Source = new BitmapImage(new System.Uri("ms-appx:///Assets/8k_logo.png"));
            }
            
            // Display AI badge if applicable
            AIBadge.Visibility = wallpaper.IsAIGenerated ? Visibility.Visible : Visibility.Collapsed;
            
            // Set likes and downloads counters
            LikesText.Text = wallpaper.Likes.ToString();
            DownloadsText.Text = wallpaper.Downloads.ToString();
        }
    }
} 