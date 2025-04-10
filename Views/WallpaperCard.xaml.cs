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
            
            // Set the image source
            WallpaperImage.Source = new BitmapImage(new System.Uri(wallpaper.ImagePath));
            
            // Set the resolution text
            ResolutionText.Text = wallpaper.Resolution;
        }
        
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // In a real app, this would download the wallpaper
            // For now, just show a simple message
            // You could implement this by raising an event that the page can handle
        }
    }
} 