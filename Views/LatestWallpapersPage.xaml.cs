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
using System.Net.Http;
using System.Text.Json;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class LatestWallpapersPage : Page
    {
        // Collection to hold the wallpapers
        private ObservableCollection<WallpaperItem> _wallpapers;
        
        // For HTTP requests
        private readonly HttpClient _httpClient = new HttpClient();
        
        // For simulating delayed loading
        private DispatcherQueue _dispatcherQueue;
        
        // Properties for infinite scrolling
        private bool _isLoading = false;
        private int _currentPage = 0;
        private int _itemsPerPage = 30; // Exactly 30 items per page as requested
        private bool _hasMoreItems = true;
        private double _loadMoreThreshold = 0.8; // 80% of the scroll viewer height
        // No max pages limit - truly infinite scrolling
        
        // Base API URL for wallpaper requests
        private const string ApiBaseUrl = "https://backiee.com/api/wallpaper/list.php";
        
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
            // Show loading indicator with 0 progress
            LoadingProgressBar.Value = 0;
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
                    LoadingProgressBar.Value = 0;
                    LoadingProgressBar.Visibility = Visibility.Visible;
                }
                
                // Brief initial delay at 0%
                LoadingProgressBar.Value = 0;
                await Task.Delay(100);
                
                // Update to 30% and stay there for longer
                LoadingProgressBar.Value = 30;
                await Task.Delay(700);
                
                // Construct API URL with the current page number
                string apiUrl = $"{ApiBaseUrl}?action=paging_list&list_type=latest&page={_currentPage}&page_size={_itemsPerPage}&category=all&is_ai=all&sort_by=popularity&4k=false&5k=false&8k=false&status=active&args=";
                
                // Fetch data from API
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                    {
                        // Process API response and create wallpaper items
                        List<WallpaperItem> newWallpapers = new List<WallpaperItem>();
                        
                        foreach (JsonElement wallpaperElement in doc.RootElement.EnumerateArray())
                        {
                            // Extract wallpaper details from JSON
                            string id = wallpaperElement.GetProperty("ID").GetString();
                            string title = wallpaperElement.GetProperty("Title").GetString();
                            string imageUrl = wallpaperElement.GetProperty("FullPhotoUrl").GetString();
                            string resolution = wallpaperElement.GetProperty("Resolution").GetString();
                            
                            // Extract tag data
                            string qualityTag = wallpaperElement.GetProperty("UltraHDType").GetString();
                            bool isAI = wallpaperElement.GetProperty("AIGenerated").GetString() == "1";
                            string likesCount = wallpaperElement.GetProperty("Rating").GetString();
                            string downloadsCount = wallpaperElement.GetProperty("Downloads").GetString();
                            
                            // Create wallpaper item
                var wallpaper = new WallpaperItem
                            {
                                Id = id,
                                Title = title,
                                ImageSource = new BitmapImage(new Uri(imageUrl)),
                                Resolution = resolution,
                                QualityTag = qualityTag,
                                IsAI = isAI,
                                Likes = likesCount,
                                Downloads = downloadsCount
                            };
                            
                            newWallpapers.Add(wallpaper);
                        }
                        
                        // Add all items at once for maximum speed
                        foreach (var wallpaper in newWallpapers)
                        {
                            _wallpapers.Add(wallpaper);
                        }
                    }
                    
                    // Increment page counter for next load
                    _currentPage++;
                    
                    // If we received fewer items than requested, we've reached the end
                    _hasMoreItems = true; // Always true for this API as it has many pages
                }
                else
                {
                    // If API request failed, mark that there are no more items
                    _hasMoreItems = false;
                }
                
                // Quick jump to 100% when complete
                LoadingProgressBar.Value = 100;
                await Task.Delay(200);
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error loading wallpapers: {ex.Message}");
                _hasMoreItems = false;
            }
            finally
            {
                _isLoading = false;
                
                // Hide loading indicator
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }
        
        private async void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Check if we need to load more items
            if (sender is ScrollViewer scrollViewer)
            {
                // Check if we're approaching the bottom
                double verticalOffset = scrollViewer.VerticalOffset;
                double maxVerticalOffset = scrollViewer.ScrollableHeight;
                
                // Load more items when we're 70% through the current content (more aggressive loading)
                // This ensures the user never reaches the bottom during normal scrolling
                if (maxVerticalOffset > 0 &&
                    verticalOffset >= maxVerticalOffset * 0.7 &&
                    !_isLoading)
                {
                    await LoadMoreWallpapers();
                    
                    // Immediately trigger another load if we're still near the bottom
                    // This helps ensure continuous content availability for fast scrollers
                    if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight * 0.85 && !_isLoading)
                    {
                        await LoadMoreWallpapers();
                    }
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
                var image = templateRoot.FindName("ItemImage") as Image; // First child should be the Image
                
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
    
    // Enhanced model for wallpaper items
    public class WallpaperItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public BitmapImage ImageSource { get; set; }
        public string Resolution { get; set; }
        
        // New properties for the tags
        public string QualityTag { get; set; } // 4K, 5K, 8K
        public bool IsAI { get; set; }
        public string Likes { get; set; }
        public string Downloads { get; set; }
        
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