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
            
            // Set up the GridView
            DailyWallpapersGridView.ItemsSource = _dailyWallpapers;
            
            // Set up preparer for wallpaper cards
            DailyWallpapersGridView.ContainerContentChanging += (sender, args) =>
            {
                if (args.ItemContainer.ContentTemplateRoot is WallpaperCard card)
                {
                    var wallpaper = args.Item as Wallpaper;
                    if (wallpaper != null)
                    {
                        card.SetWallpaper(wallpaper);
                    }
                }
            };
        }
        
        private void LatestWallpapers_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Navigate to the Latest Wallpapers page
            this.Frame.Navigate(typeof(LatestWallpapersPage));
        }

        private void DailyWallpapersWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ItemsWrapGrid wrapGrid)
            {
                // Calculate number of columns based on available width
                double availableWidth = e.NewSize.Width;
                
                // Determine desired item width (considering margins)
                double desiredItemWidth = 280;  // Base item width
                double itemMargin = 4;          // Total margin between items
                
                // Calculate how many items can fit in the available width
                int columnsCount = Math.Max(1, (int)(availableWidth / (desiredItemWidth + itemMargin)));
                
                // Ensure we have a reasonable column count
                columnsCount = Math.Min(columnsCount, 6);  // Limit to maximum 6 columns
                
                // Set the maximum columns
                wrapGrid.MaximumRowsOrColumns = columnsCount;
                
                // Calculate the new item width to fill the available space evenly
                // Accounting for margins between items
                double totalMarginWidth = (columnsCount - 1) * itemMargin;
                double newItemWidth = (availableWidth - totalMarginWidth) / columnsCount;
                
                // Set item width with a minimum threshold
                wrapGrid.ItemWidth = Math.Max(180, newItemWidth);
                
                // Ensure alignment stretches to use all available space
                wrapGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }
    }
} 