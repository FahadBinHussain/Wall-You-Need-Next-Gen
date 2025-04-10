using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Dispatching;
using System.Threading.Tasks;
using Wall_You_Need_Next_Gen.Models;
using System.Windows.Input;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class LatestWallpapersPage : Page
    {
        // Collection to hold the wallpapers
        private ObservableCollection<WallpaperItem> _wallpapers;
        
        // For simulating delayed loading
        private DispatcherQueue _dispatcherQueue;
        
        public LatestWallpapersPage()
        {
            this.InitializeComponent();
            
            // Initialize the wallpapers collection
            _wallpapers = new ObservableCollection<WallpaperItem>();
            
            // Get the dispatcher queue for this thread
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            
            // Register for events
            Loaded += LatestWallpapersPage_Loaded;
            Unloaded += LatestWallpapersPage_Unloaded;
        }
        
        private async void LatestWallpapersPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Show loading indicators
            StatusTextBlock.Visibility = Visibility.Visible;
            LoadingProgressRing.Visibility = Visibility.Visible;
            
            // Initialize the GridView with wallpapers collection
            WallpapersGridView.ItemsSource = _wallpapers;
            
            // Load placeholder wallpapers
            await LoadPlaceholderWallpapers();
            
            // Hide loading indicators
            StatusTextBlock.Visibility = Visibility.Collapsed;
            LoadingProgressRing.Visibility = Visibility.Collapsed;
        }
        
        private void LatestWallpapersPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up
            _wallpapers.Clear();
        }
        
        private async Task LoadPlaceholderWallpapers()
        {
            // Clear existing items
            _wallpapers.Clear();
            
            // Add placeholder wallpapers (3 rows x 4 columns = 12 items)
            for (int i = 0; i < 12; i++)
            {
                // Create a simulated wallpaper with placeholder image
                var wallpaper = new WallpaperItem
                {
                    ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-dark.png")),
                    Resolution = GetRandomResolution(),
                    Id = i.ToString()
                };
                
                // Add to collection
                _wallpapers.Add(wallpaper);
                
                // Small delay for smoother loading appearance
                await Task.Delay(50);
            }
        }
        
        private string GetRandomResolution()
        {
            // Generate random resolution for placeholder data
            string[] resolutions = { "1920x1080", "2560x1440", "3840x2160 (4K)", "5120x2880 (5K)", "7680x4320 (8K)" };
            Random random = new Random();
            return resolutions[random.Next(resolutions.Length)];
        }
        
        private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // This would be implemented for infinite scrolling
            // Not implemented in this placeholder version
        }
        
        private void WallpapersGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is WallpaperItem wallpaper)
            {
                // Show a simple message for now
                StatusTextBlock.Text = $"Selected wallpaper: ID {wallpaper.Id}";
                StatusTextBlock.Visibility = Visibility.Visible;
                
                // Hide status after 3 seconds
                _dispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Delay(3000);
                    StatusTextBlock.Visibility = Visibility.Collapsed;
                });
            }
        }
        
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a simple message for now
            StatusTextBlock.Text = "Filter button clicked";
            StatusTextBlock.Visibility = Visibility.Visible;
            
            // Hide status after 3 seconds
            _dispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(3000);
                StatusTextBlock.Visibility = Visibility.Collapsed;
            });
        }
        
        private void SetAsSlideshowButton_Click(object sender, RoutedEventArgs e)
        {
            // Show a simple message for now
            StatusTextBlock.Text = "Set as slideshow button clicked";
            StatusTextBlock.Visibility = Visibility.Visible;
            
            // Hide status after 3 seconds
            _dispatcherQueue.TryEnqueue(async () =>
            {
                await Task.Delay(3000);
                StatusTextBlock.Visibility = Visibility.Collapsed;
            });
        }
        
        private void WallpapersWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
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
                
                // Ensure we have a reasonable column count based on screen size
                columnsCount = Math.Min(columnsCount, 6);  // Limit to maximum 6 columns
                
                // Set the maximum columns
                wrapGrid.MaximumRowsOrColumns = columnsCount;
                
                // Calculate the new item width to fill the available space evenly
                // Accounting for margins between items
                double totalMarginWidth = (columnsCount - 1) * itemMargin;
                double newItemWidth = (availableWidth - totalMarginWidth) / columnsCount;
                
                // Ensure the width is not too small to maintain quality
                double finalWidth = Math.Max(180, newItemWidth);
                
                // Calculate proportional height based on typical wallpaper aspect ratio (16:9)
                double aspectRatio = 16.0 / 9.0;
                double finalHeight = finalWidth / aspectRatio;
                
                // Set item dimensions
                wrapGrid.ItemWidth = finalWidth;
                wrapGrid.ItemHeight = finalHeight + 50; // Add extra space for the info panel at bottom
                
                // Ensure alignment stretches to use all available space
                wrapGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }
    }
    
    // Model for wallpaper items
    public class WallpaperItem
    {
        public string Id { get; set; }
        public BitmapImage ImageSource { get; set; }
        public string Resolution { get; set; }
        public bool IsAI { get; set; }
        public ICommand DownloadCommand { get; set; }
        
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