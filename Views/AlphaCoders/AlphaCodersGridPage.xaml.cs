using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Wall_You_Need_Next_Gen.Models;
using Wall_You_Need_Next_Gen.Services;

namespace Wall_You_Need_Next_Gen.Views.AlphaCoders
{
    public sealed partial class AlphaCodersGridPage : Page
    {
        private ObservableCollection<WallpaperItem> _wallpapers = new ObservableCollection<WallpaperItem>();
        private AlphaCodersService _alphaCodersService;
        private int _currentPage = 1;
        private bool _isLoading = false;
        private bool _hasMoreWallpapers = true;

        public AlphaCodersGridPage()
        {
            this.InitializeComponent();
            _alphaCodersService = new AlphaCodersService();
            WallpapersGridView.ItemsSource = _wallpapers;
            LoadWallpapers();
        }

        private async void LoadWallpapers()
        {
            if (_isLoading || !_hasMoreWallpapers)
                return;

            try
            {
                _isLoading = true;
                LoadingProgressBar.Visibility = Visibility.Visible;

                var newWallpapers = await _alphaCodersService.GetLatestWallpapersAsync(_currentPage);
                
                if (newWallpapers.Count == 0)
                {
                    _hasMoreWallpapers = false;
                }
                else
                {
                    foreach (var wallpaper in newWallpapers)
                    {
                        _wallpapers.Add(wallpaper);
                    }
                    _currentPage++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading wallpapers: {ex.Message}");
            }
            finally
            {
                _isLoading = false;
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // Load more wallpapers when scrolling near the bottom
            if (!e.IsIntermediate && MainScrollViewer.VerticalOffset >= MainScrollViewer.ScrollableHeight - 200)
            {
                LoadWallpapers();
            }
        }

        private void WallpapersGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is WallpaperItem wallpaper)
            {
                // Navigate to detail page
                Frame.Navigate(typeof(Wall_You_Need_Next_Gen.Views.AlphaCoders.AlphaCodersDetailPage), wallpaper);
            }
        }

        private void WallpapersGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
                return;

            if (args.ItemIndex >= 0 && args.Item is WallpaperItem wallpaper)
            {
                // Load the image asynchronously
                if (wallpaper.ImageSource == null)
                {
                    args.RegisterUpdateCallback(async (s, e) =>
                    {
                        if (e.Item is WallpaperItem w && w.ImageSource == null)
                        {
                            try
                            {
                                // For placeholder images, create a BitmapImage directly
                                if (w.ImageUrl.StartsWith("ms-appx:"))
                                {
                                    var bitmap = new BitmapImage(new Uri(w.ImageUrl));
                                    w.ImageSource = bitmap;
                                }
                                else
                                {
                                    // For remote images, load asynchronously
                                    w.ImageSource = await w.LoadImageAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error loading image: {ex.Message}");
                            }
                        }
                    });
                }
            }
        }

        private void WallpapersWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)sender;
            var containerWidth = e.NewSize.Width;

            // Calculate the number of columns based on available width
            // Adjust these values based on your design preferences
            double desiredItemWidth = 300; // Target width for each item
            double minItemWidth = 200;     // Minimum width for each item

            // Calculate how many items can fit in a row
            int columns = Math.Max(1, (int)(containerWidth / desiredItemWidth));
            double itemWidth = Math.Max(minItemWidth, containerWidth / columns - 16); // 16 for margins

            // Set the item width and height (maintain aspect ratio 16:9)
            panel.ItemWidth = itemWidth;
            panel.ItemHeight = itemWidth * 9 / 16;
        }
    }
}