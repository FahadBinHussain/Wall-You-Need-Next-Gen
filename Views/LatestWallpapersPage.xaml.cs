using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Wall_You_Need_Next_Gen.Models;
using Wall_You_Need_Next_Gen.Services;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class LatestWallpapersPage : Page
    {
        private WallpaperService _wallpaperService;

        public LatestWallpapersPage()
        {
            this.InitializeComponent();
            _wallpaperService = new WallpaperService();
            LoadWallpapers();
        }

        private void LoadWallpapers()
        {
            // Get the latest wallpapers from the service
            var wallpapers = _wallpaperService.GetLatestWallpapers(12);

            // Add wallpaper cards to the grid
            for (int i = 0; i < wallpapers.Count; i++)
            {
                var row = i / 3;
                var column = i % 3;
                
                var card = new WallpaperCard();
                card.SetWallpaper(wallpapers[i]);
                card.Margin = new Thickness(8);
                card.Height = 180;
                
                Grid.SetRow(card, row);
                Grid.SetColumn(card, column);
                
                WallpapersGrid.Children.Add(card);
            }
        }
    }
} 