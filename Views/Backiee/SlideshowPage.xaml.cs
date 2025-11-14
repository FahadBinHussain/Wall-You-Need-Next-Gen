using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Wall_You_Need_Next_Gen.Services;

namespace Wall_You_Need_Next_Gen.Views.Backiee
{
    public sealed partial class SlideshowPage : Page
    {
        // Desktop slideshow settings
        private bool _desktopSlideshowEnabled = false;
        private string _desktopPlatform = "";
        private string _desktopCategory = "";

        // Lock screen slideshow settings
        private bool _lockScreenSlideshowEnabled = false;
        private string _lockScreenPlatform = "";
        private string _lockScreenCategory = "";

        // Shared refresh interval
        private string _refreshInterval = "12 hours";

        public SlideshowPage()
        {
            this.InitializeComponent();
            LoadSettings();
        }

        private void LogInfo(string message)
        {
            try
            {
                ((App)Application.Current).LogInfo($"[SlideshowPage] {message}");
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine($"[SlideshowPage] {message}");
            }
        }

        private void ExpandDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement expand functionality
            LogInfo("Expand desktop slideshow clicked");
        }

        private async void NextDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await SlideshowService.Instance.NextDesktopWallpaper();
            LogInfo("Next desktop slideshow clicked");
        }

        private async void EditDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await ShowSlideshowSettingsDialog("Desktop");
        }

        private async void ScheduleDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await ShowScheduleDialog("Desktop");
        }

        private void ExpandLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement expand functionality
            LogInfo("Expand lock screen slideshow clicked");
        }

        private async void NextLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await SlideshowService.Instance.NextLockScreenWallpaper();
            LogInfo("Next lock screen slideshow clicked");
        }

        private async void EditLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await ShowSlideshowSettingsDialog("Lock Screen");
        }

        private async void ScheduleLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await ShowScheduleDialog("Lock Screen");
        }

        private void LoadSettings()
        {
            // Update desktop slideshow status
            if (_desktopSlideshowEnabled && !string.IsNullOrEmpty(_desktopPlatform) && !string.IsNullOrEmpty(_desktopCategory))
            {
                DesktopStatusText.Text = $"{_desktopPlatform} - {_desktopCategory}";
            }
            else
            {
                DesktopStatusText.Text = "No slideshow set";
            }

            // Update lock screen slideshow status
            if (_lockScreenSlideshowEnabled && !string.IsNullOrEmpty(_lockScreenPlatform) && !string.IsNullOrEmpty(_lockScreenCategory))
            {
                LockScreenStatusText.Text = $"{_lockScreenPlatform} - {_lockScreenCategory}";
            }
            else
            {
                LockScreenStatusText.Text = "No slideshow set";
            }
        }

        private async System.Threading.Tasks.Task ShowSlideshowSettingsDialog(string slideshowType)
        {
            // Create the content dialog
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = $"Set {slideshowType.ToLower()} slideshow",
                PrimaryButtonText = "Set",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            // Create the content
            var contentPanel = new StackPanel
            {
                Spacing = 16,
                Margin = new Thickness(0, 12, 0, 12)
            };

            // Enable slideshow toggle
            var toggleCard = new Border
            {
                Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12)
            };

            var toggleGrid = new Grid();
            toggleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            toggleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var toggleLabel = new TextBlock
            {
                Text = "Enable slideshow",
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16
            };
            Grid.SetColumn(toggleLabel, 0);

            // Load existing settings for this slideshow type
            bool currentEnabled = slideshowType == "Desktop" ? _desktopSlideshowEnabled : _lockScreenSlideshowEnabled;
            string currentPlatform = slideshowType == "Desktop" ? _desktopPlatform : _lockScreenPlatform;
            string currentCategory = slideshowType == "Desktop" ? _desktopCategory : _lockScreenCategory;

            var toggleSwitch = new ToggleSwitch
            {
                IsOn = currentEnabled,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(toggleSwitch, 1);

            toggleGrid.Children.Add(toggleLabel);
            toggleGrid.Children.Add(toggleSwitch);
            toggleCard.Child = toggleGrid;
            contentPanel.Children.Add(toggleCard);

            // Change slideshow section
            var changeSlideshowLabel = new TextBlock
            {
                Text = "Change slideshow",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                FontSize = 16,
                Margin = new Thickness(0, 8, 0, 4)
            };
            contentPanel.Children.Add(changeSlideshowLabel);

            // Platform dropdown
            var platformComboBox = new ComboBox
            {
                Header = "Select Platform",
                PlaceholderText = "Choose a platform",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 400
            };
            platformComboBox.Items.Add("Backiee");
            platformComboBox.Items.Add("AlphaCoders");
            
            // Set selected platform based on saved settings
            if (!string.IsNullOrEmpty(currentPlatform))
            {
                int platformIndex = currentPlatform == "AlphaCoders" ? 1 : 0;
                platformComboBox.SelectedIndex = platformIndex;
            }
            else
            {
                platformComboBox.SelectedIndex = 0;
            }
            
            contentPanel.Children.Add(platformComboBox);

            // Category dropdown
            var categoryComboBox = new ComboBox
            {
                Header = "Select Category",
                PlaceholderText = "Choose a category",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 400
            };
            
            // Update categories when platform changes
            bool isInitializing = true;
            platformComboBox.SelectionChanged += (s, e) =>
            {
                categoryComboBox.Items.Clear();
                if (platformComboBox.SelectedIndex == 0) // Backiee
                {
                    categoryComboBox.Items.Add("Latest Wallpapers");
                    categoryComboBox.Items.Add("8K UltraHD");
                    categoryComboBox.Items.Add("AI Generated");
                }
                else // AlphaCoders
                {
                    categoryComboBox.Items.Add("4K Wallpapers");
                    categoryComboBox.Items.Add("Harvest Wallpapers");
                    categoryComboBox.Items.Add("Rain Wallpapers");
                }
                
                // Set selected category if we have a saved one and we're initializing
                if (isInitializing && !string.IsNullOrEmpty(currentCategory))
                {
                    for (int i = 0; i < categoryComboBox.Items.Count; i++)
                    {
                        if (categoryComboBox.Items[i]?.ToString() == currentCategory)
                        {
                            categoryComboBox.SelectedIndex = i;
                            isInitializing = false;
                            return;
                        }
                    }
                }
                
                if (categoryComboBox.Items.Count > 0)
                {
                    categoryComboBox.SelectedIndex = 0;
                }
                isInitializing = false;
            };
            
            // Initialize with current platform's categories
            if (currentPlatform == "AlphaCoders")
            {
                categoryComboBox.Items.Add("4K Wallpapers");
                categoryComboBox.Items.Add("Harvest Wallpapers");
                categoryComboBox.Items.Add("Rain Wallpapers");
            }
            else
            {
                categoryComboBox.Items.Add("Latest Wallpapers");
                categoryComboBox.Items.Add("8K UltraHD");
                categoryComboBox.Items.Add("AI Generated");
            }
            
            // Set selected category based on saved settings
            if (!string.IsNullOrEmpty(currentCategory))
            {
                for (int i = 0; i < categoryComboBox.Items.Count; i++)
                {
                    if (categoryComboBox.Items[i]?.ToString() == currentCategory)
                    {
                        categoryComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (categoryComboBox.Items.Count > 0)
            {
                categoryComboBox.SelectedIndex = 0;
            }
            
            isInitializing = false;
            
            contentPanel.Children.Add(categoryComboBox);

            dialog.Content = contentPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Save slideshow settings
                bool isEnabled = toggleSwitch.IsOn;
                string selectedPlatform = platformComboBox.SelectedItem?.ToString() ?? "Backiee";
                string selectedCategory = categoryComboBox.SelectedItem?.ToString() ?? "Latest Wallpapers";

                LogInfo($"========== {slideshowType} Slideshow Settings Dialog - SET CLICKED ==========");
                LogInfo($"  Enabled: {isEnabled}");
                LogInfo($"  Platform: {selectedPlatform}");
                LogInfo($"  Category: {selectedCategory}");
                LogInfo($"  Current Refresh Interval: {_refreshInterval}");

                // Save to class fields and start/stop slideshow
                if (slideshowType == "Desktop")
                {
                    _desktopSlideshowEnabled = isEnabled;
                    _desktopPlatform = selectedPlatform;
                    _desktopCategory = selectedCategory;
                    
                    LogInfo($"Saved desktop settings. Enabled={_desktopSlideshowEnabled}, Platform={_desktopPlatform}, Category={_desktopCategory}");
                    
                    DesktopStatusText.Text = isEnabled 
                        ? $"{selectedPlatform} - {selectedCategory}" 
                        : "No slideshow set";

                    // Start or stop slideshow
                    if (isEnabled && !string.IsNullOrEmpty(_desktopPlatform) && !string.IsNullOrEmpty(_desktopCategory))
                    {
                        LogInfo($"Starting desktop slideshow...");
                        var interval = SlideshowService.ParseInterval(_refreshInterval);
                        LogInfo($"Parsed interval: {interval}");
                        await SlideshowService.Instance.StartDesktopSlideshow(_desktopPlatform, _desktopCategory, interval, this.DispatcherQueue);
                        LogInfo($"Desktop slideshow start command completed");
                    }
                    else
                    {
                        LogInfo($"Stopping desktop slideshow...");
                        SlideshowService.Instance.StopDesktopSlideshow();
                    }
                }
                else
                {
                    _lockScreenSlideshowEnabled = isEnabled;
                    _lockScreenPlatform = selectedPlatform;
                    _lockScreenCategory = selectedCategory;
                    
                    LockScreenStatusText.Text = isEnabled 
                        ? $"{selectedPlatform} - {selectedCategory}" 
                        : "No slideshow set";

                    // Start or stop slideshow
                    if (isEnabled && !string.IsNullOrEmpty(_lockScreenPlatform) && !string.IsNullOrEmpty(_lockScreenCategory))
                    {
                        var interval = SlideshowService.ParseInterval(_refreshInterval);
                        await SlideshowService.Instance.StartLockScreenSlideshow(_lockScreenPlatform, _lockScreenCategory, interval, this.DispatcherQueue);
                    }
                    else
                    {
                        SlideshowService.Instance.StopLockScreenSlideshow();
                    }
                }
            }
        }

        private async System.Threading.Tasks.Task ShowScheduleDialog(string slideshowType)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Slideshow refresh interval",
                PrimaryButtonText = "Set",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            // Create the content panel
            var contentPanel = new StackPanel
            {
                Spacing = 16,
                Margin = new Thickness(0, 12, 0, 12)
            };

            // Description text
            var descriptionText = new TextBlock
            {
                Text = "This setting applies to both desktop and lock screen slideshows.",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14
            };

            // Interval dropdown
            var intervalComboBox = new ComboBox
            {
                PlaceholderText = "Select interval",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                MinWidth = 400
            };

            // Add interval options
            intervalComboBox.Items.Add("30 minutes");
            intervalComboBox.Items.Add("1 hour");
            intervalComboBox.Items.Add("3 hours");
            intervalComboBox.Items.Add("6 hours");
            intervalComboBox.Items.Add("12 hours");
            intervalComboBox.Items.Add("24 hours");

            // Set to saved interval or default to 12 hours
            int savedIndex = 4; // default to 12 hours
            for (int i = 0; i < intervalComboBox.Items.Count; i++)
            {
                if (intervalComboBox.Items[i]?.ToString() == _refreshInterval)
                {
                    savedIndex = i;
                    break;
                }
            }
            intervalComboBox.SelectedIndex = savedIndex;

            contentPanel.Children.Add(descriptionText);
            contentPanel.Children.Add(intervalComboBox);

            dialog.Content = contentPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && intervalComboBox.SelectedItem != null)
            {
                string selectedInterval = intervalComboBox.SelectedItem.ToString();
                
                // Save to class field
                _refreshInterval = selectedInterval;
                var interval = SlideshowService.ParseInterval(selectedInterval);
                
                // Restart desktop slideshow with new interval if enabled
                if (_desktopSlideshowEnabled && !string.IsNullOrEmpty(_desktopPlatform) && !string.IsNullOrEmpty(_desktopCategory))
                {
                    await SlideshowService.Instance.StartDesktopSlideshow(_desktopPlatform, _desktopCategory, interval, this.DispatcherQueue);
                    DesktopStatusText.Text = $"{_desktopPlatform} - {_desktopCategory} (Refresh: {selectedInterval})";
                }
                
                // Restart lock screen slideshow with new interval if enabled
                if (_lockScreenSlideshowEnabled && !string.IsNullOrEmpty(_lockScreenPlatform) && !string.IsNullOrEmpty(_lockScreenCategory))
                {
                    await SlideshowService.Instance.StartLockScreenSlideshow(_lockScreenPlatform, _lockScreenCategory, interval, this.DispatcherQueue);
                    LockScreenStatusText.Text = $"{_lockScreenPlatform} - {_lockScreenCategory} (Refresh: {selectedInterval})";
                }
            }
        }
    }
}
