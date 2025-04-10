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
using System.Linq;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class LatestWallpapersPage : Page
    {
        // Collection to hold the wallpapers
        private ObservableCollection<WallpaperItem> _wallpapers;
        
        // For simulating delayed loading
        private DispatcherQueue _dispatcherQueue;
        
        // Properties for infinite scrolling
        private bool _isLoading = false;
        private int _currentPage = 0;
        private int _itemsPerPage = 30; // Exactly 30 items per page as requested
        private bool _hasMoreItems = true;
        private double _loadMoreThreshold = 0.8; // 80% of the scroll viewer height
        private int _maxPages = 3; // Load exactly 3 pages (30 + 30 + 30 = 90 total)
        
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
            // Show loading indicator
            LoadingProgressBar.Visibility = Visibility.Visible;
            
            // Initialize the GridView with wallpapers collection
            WallpapersGridView.ItemsSource = _wallpapers;
            
            // Reset paging variables
            _currentPage = 0;
            _hasMoreItems = true;
            _wallpapers.Clear();
            
            // Load first page of wallpapers
            await LoadMoreWallpapers();
            
            // Hide loading indicator after first batch is loaded
            LoadingProgressBar.Visibility = Visibility.Collapsed;
        }
        
        private void LatestWallpapersPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clean up
            _wallpapers.Clear();
        }
        
        private async Task LoadPlaceholderWallpapers()
        {
            // This method is replaced by the LoadMoreWallpapers method
            await LoadMoreWallpapers();
        }
        
        private async Task LoadMoreWallpapers()
        {
            // Prevent multiple concurrent loading operations
            if (_isLoading || !_hasMoreItems)
                return;
            
            try
            {
                _isLoading = true;
                
                // Show loading indicator for subsequent page loads
                if (_currentPage > 0)
                {
                    LoadingProgressBar.Visibility = Visibility.Visible;
                }
                
                // Increment the page counter
                _currentPage++;
                
                // In a real app, you would fetch items from an API or database
                // Here we'll simulate loading with placeholder images
                int startIndex = (_currentPage - 1) * _itemsPerPage;
                
                // Simulate network delay - but just once per batch instead of per item
                await Task.Delay(1500);
                
                // Create batch of wallpapers first
                List<WallpaperItem> newWallpapers = new List<WallpaperItem>(_itemsPerPage);
                
                // Prepare all wallpaper items (without adding to collection yet)
                for (int i = 0; i < _itemsPerPage; i++)
                {
                    int itemId = startIndex + i;
                    
                    // Create a simulated wallpaper with placeholder image
                    var wallpaper = new WallpaperItem
                    {
                        ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-dark.png")),
                        Resolution = GetRandomResolution(),
                        Id = itemId.ToString()
                    };
                    
                    newWallpapers.Add(wallpaper);
                }
                
                // Add all wallpapers in small batches to keep UI responsive
                const int batchSize = 6; // Load 6 at a time for smoother UI updates
                for (int i = 0; i < newWallpapers.Count; i += batchSize)
                {
                    // Add a batch of items
                    for (int j = 0; j < batchSize && i + j < newWallpapers.Count; j++)
                    {
                        _wallpapers.Add(newWallpapers[i + j]);
                    }
                    
                    // Small delay between batches for smoother loading appearance
                    await Task.Delay(100);
                }
                
                // Stop after exactly 3 pages (30 + 30 + 30 = 90 total wallpapers)
                if (_currentPage >= _maxPages)
                {
                    _hasMoreItems = false;
                }
            }
            finally
            {
                _isLoading = false;
                
                // Hide loading indicator
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }
        
        private string GetRandomResolution()
        {
            // Generate random resolution for placeholder data
            string[] resolutions = { "1920x1080", "2560x1440", "3840x2160 (4K)", "5120x2880 (5K)", "7680x4320 (8K)" };
            Random random = new Random();
            return resolutions[random.Next(resolutions.Length)];
        }
        
        private async void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // If it's not a direct manipulation (such as a programmatic scroll), ignore
            if (!e.IsIntermediate) 
                return;
            
            // Check if we need to load more items
            if (sender is ScrollViewer scrollViewer)
            {
                // Check if we're near the bottom
                double verticalOffset = scrollViewer.VerticalOffset;
                double maxVerticalOffset = scrollViewer.ScrollableHeight;
                
                // If we've scrolled past the threshold and not loading, load more
                if (maxVerticalOffset > 0 &&
                    verticalOffset >= maxVerticalOffset * _loadMoreThreshold &&
                    !_isLoading && 
                    _hasMoreItems)
                {
                    await LoadMoreWallpapers();
                }
            }
        }
        
        private void WallpapersGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is WallpaperItem wallpaper)
            {
                // Show a simple message - without using text display, just log to debug
                System.Diagnostics.Debug.WriteLine($"Selected wallpaper: ID {wallpaper.Id}");
            }
        }
        
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Just log to debug
            System.Diagnostics.Debug.WriteLine("Filter button clicked");
        }
        
        private void SetAsSlideshowButton_Click(object sender, RoutedEventArgs e)
        {
            // Just log to debug
            System.Diagnostics.Debug.WriteLine("Set as slideshow button clicked");
        }
        
        private void WallpapersWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is ItemsWrapGrid wrapGrid)
            {
                // Calculate number of columns based on available width
                double availableWidth = e.NewSize.Width;
                
                // Determine desired item width (considering margins)
                double desiredItemWidth = 300;  // Base item width
                double itemMargin = 8;          // Total margin between items (4px on each side)
                
                // Calculate how many items can fit in the available width
                int columnsCount = Math.Max(1, (int)(availableWidth / desiredItemWidth));
                
                // Ensure we have a reasonable column count based on screen size
                columnsCount = Math.Min(columnsCount, 6);  // Limit to maximum 6 columns
                
                // Set the maximum columns
                wrapGrid.MaximumRowsOrColumns = columnsCount;
                
                // Calculate the new item width to fill the available space with margins
                double totalMarginWidth = (columnsCount - 1) * itemMargin;
                double newItemWidth = (availableWidth - totalMarginWidth) / columnsCount;
                
                // Set a reasonable minimum width
                double finalWidth = Math.Max(200, newItemWidth);
                
                // Calculate proportional height based on typical wallpaper aspect ratio (16:9)
                double aspectRatio = 16.0 / 9.0;
                double finalHeight = finalWidth / aspectRatio;
                
                // Set item dimensions - use only the image height since we removed the info panel
                wrapGrid.ItemWidth = finalWidth;
                wrapGrid.ItemHeight = finalHeight;
                
                // Make sure the grid fills all available space
                wrapGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            }
        }
        
        private void WallpapersGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                // Clear image from items that are being recycled
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = templateRoot.FindName("ItemImage") as Image;
                if (image != null)
                {
                    image.Source = null;
                }
            }
            
            if (args.Phase == 0)
            {
                // Register for the next phase to load the image
                args.RegisterUpdateCallback(ShowImage);
                args.Handled = true;
            }
        }
        
        private void ShowImage(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.Phase == 1)
            {
                // Find the Image control in the template
                var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
                var image = templateRoot.Children[0] as Image; // First child should be the Image
                
                // Get the wallpaper item
                var wallpaper = args.Item as WallpaperItem;
                
                // Set the image source
                if (image != null && wallpaper != null)
                {
                    image.Source = wallpaper.ImageSource;
                }
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