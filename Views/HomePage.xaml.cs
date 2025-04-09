using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Wall_You_Need_Next_Gen.Models;
using Wall_You_Need_Next_Gen.Services;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class HomePage : Page
    {
        private WallpaperService _wallpaperService;
        private ObservableCollection<Wallpaper> _dailyWallpapers;
        
        public HomePage()
        {
            this.InitializeComponent();
            _wallpaperService = new WallpaperService();
            _dailyWallpapers = new ObservableCollection<Wallpaper>();
            
            Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBannerImages();
            LoadDailyPopularWallpapers();
        }

        private void LoadBannerImages()
        {
            try
            {
                // Use the available assets for banner images
                LatestBannerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-dark.png"));
                UltraHdImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-dark.png"));
                AiGeneratedImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-dark.png"));
                AiChatWidgetImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/noad.png"));
            }
            catch (Exception ex)
            {
                // Use placeholder if image loading fails
                LatestBannerImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
                UltraHdImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
                AiGeneratedImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
                AiChatWidgetImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
            }
        }

        private void LoadDailyPopularWallpapers()
        {
            // Get daily popular wallpapers from service
            var wallpapers = _wallpaperService.GetDailyPopularWallpapers();
            
            // Clear existing collection
            _dailyWallpapers.Clear();
            
            // Add wallpapers to collection
            foreach (var wallpaper in wallpapers)
            {
                _dailyWallpapers.Add(wallpaper);
            }
            
            // Set up the ItemsRepeater
            DailyWallpapersRepeater.ItemsSource = _dailyWallpapers;
            
            // Create element factory to build WallpaperCard controls
            DailyWallpapersRepeater.ElementPrepared += (sender, args) =>
            {
                if (args.Element is WallpaperCard card)
                {
                    var wallpaper = _dailyWallpapers[args.Index];
                    card.SetWallpaper(wallpaper);
                }
            };
        }
        
        private void LatestWallpapers_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Navigate to the Latest Wallpapers page
            this.Frame.Navigate(typeof(LatestWallpapersPage));
        }
    }
} 