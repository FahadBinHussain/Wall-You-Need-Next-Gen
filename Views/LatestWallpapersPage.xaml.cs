using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using Wall_You_Need_Next_Gen.Models;
using Wall_You_Need_Next_Gen.Services;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class LatestWallpapersPage : Page
    {
        private WallpaperService _wallpaperService;
        private double _lastWidth = 0;
        private const double ItemMinWidth = 220; // Minimum width for cards
        private const double ItemDefaultWidth = 300; // Default width for cards
        private const double ItemMargin = 16; // Total margin (8px on each side)
        
        public LatestWallpapersPage()
        {
            this.InitializeComponent();
            
            _wallpaperService = new WallpaperService();
            
            // Load data when page is loaded
            Loaded += OnLoaded;
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadPlaceholderWallpapers();
            
            // Do initial sizing based on current width
            if (ActualWidth > 0)
            {
                UpdateGridLayout(ActualWidth);
            }
        }
        
        // Event handler for GridView's SizeChanged event (defined in XAML)
        private void WallpapersGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                // Skip small changes to avoid unnecessary layouts
                if (Math.Abs(_lastWidth - e.NewSize.Width) < 20)
                    return;
                
                _lastWidth = e.NewSize.Width;
                
                // Update grid layout based on new width
                UpdateGridLayout(e.NewSize.Width);
                
                // Animate the transition
                var storyboard = Resources["ResizeAnimation"] as Storyboard;
                storyboard?.Begin();
            }
            catch (Exception ex)
            {
                // Ignore animation errors but log them
                System.Diagnostics.Debug.WriteLine($"Layout update error: {ex.Message}");
            }
        }
        
        private void UpdateGridLayout(double availableWidth)
        {
            try
            {
                // Get the wrap grid from the panel
                if (WallpapersGridView?.ItemsPanelRoot is ItemsWrapGrid wrapGrid)
                {
                    // Account for container margins
                    double effectiveWidth = Math.Max(0, availableWidth - 48);
                    
                    // Calculate optimal number of columns based on available width
                    int columns = Math.Max(1, (int)(effectiveWidth / (ItemDefaultWidth + ItemMargin)));
                    
                    // Set the maximum columns
                    wrapGrid.MaximumRowsOrColumns = columns;
                    
                    // Optionally, we can also resize the items to better fit the container
                    // This is commented out since we're using fixed sizes in the XAML template
                    // The code would resize the WallpaperCard instances in the GridView
                    
                    /*
                    double itemWidth = Math.Max(ItemMinWidth, (effectiveWidth / columns) - ItemMargin);
                    foreach (var item in WallpapersGridView.Items)
                    {
                        if (WallpapersGridView.ContainerFromItem(item) is GridViewItem container &&
                            container.ContentTemplateRoot is WallpaperCard card)
                        {
                            card.Width = itemWidth;
                            card.Height = itemWidth * 0.6; // 5:3 aspect ratio
                        }
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating grid layout: {ex.Message}");
            }
        }
        
        private void LoadPlaceholderWallpapers()
        {
            try
            {
                var wallpapers = CreatePlaceholderWallpapers();
                
                // Clear existing items
                WallpapersGridView.Items.Clear();
                
                // Add wallpaper cards to the GridView
                foreach (var wallpaper in wallpapers)
                {
                    var card = new WallpaperCard();
                    card.SetWallpaper(wallpaper);
                    WallpapersGridView.Items.Add(card);
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                System.Diagnostics.Debug.WriteLine($"Error loading wallpapers: {ex.Message}");
            }
        }
        
        private List<Wallpaper> CreatePlaceholderWallpapers()
        {
            // Use the wallpaper service to get placeholder data
            return _wallpaperService.GetLatestWallpapers(12);
        }
    }
} 