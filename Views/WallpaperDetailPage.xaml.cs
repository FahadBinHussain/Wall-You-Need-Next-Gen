using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml;
using Wall_You_Need_Next_Gen.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Wall_You_Need_Next_Gen.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WallpaperDetailPage : Page
    {
        public WallpaperDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is WallpaperItem wallpaper && wallpaper != null)
            {
                TitleTextBlock.Text = wallpaper.Title;

                if (!string.IsNullOrEmpty(wallpaper.FullPhotoUrl))
                {
                    try
                    {
                        WallpaperImage.Source = new BitmapImage(new Uri(wallpaper.FullPhotoUrl));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting WallpaperImage source: {ex.Message}");
                        WallpaperImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-wallpaper-1000.jpg"));
                    }
                }
                else
                {
                    WallpaperImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-wallpaper-1000.jpg"));
                }

                AITagBorder.Visibility = wallpaper.IsAI ? Visibility.Visible : Visibility.Collapsed;
                
                if (!string.IsNullOrEmpty(wallpaper.QualityTag) && (wallpaper.QualityTag == "4K" || wallpaper.QualityTag == "8K"))
                {
                    QualityTagBorder.Visibility = Visibility.Visible;
                    QualityTagText.Text = wallpaper.QualityTag;
                    if (!string.IsNullOrEmpty(wallpaper.QualityLogoPath))
                    {
                        try
                        {
                            QualityImage.Source = new BitmapImage(new Uri(wallpaper.QualityLogoPath));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error setting QualityImage source: {ex.Message}");
                            QualityTagBorder.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        QualityTagBorder.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    QualityTagBorder.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                TitleTextBlock.Text = "Error Loading Wallpaper";
                WallpaperImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder-wallpaper-1000.jpg"));
                AITagBorder.Visibility = Visibility.Collapsed;
                QualityTagBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
} 