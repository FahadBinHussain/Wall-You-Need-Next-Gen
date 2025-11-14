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

            // Horizontal panel for number + unit
            var intervalPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12
            };

            // Number input
            var numberBox = new Microsoft.UI.Xaml.Controls.NumberBox
            {
                PlaceholderText = "Enter value",
                Minimum = 1,
                Maximum = 999,
                SpinButtonPlacementMode = Microsoft.UI.Xaml.Controls.NumberBoxSpinButtonPlacementMode.Inline,
                Width = 200
            };

            // Unit dropdown
            var unitComboBox = new ComboBox
            {
                PlaceholderText = "Unit",
                MinWidth = 150
            };

            // Add unit options
            unitComboBox.Items.Add("Seconds");
            unitComboBox.Items.Add("Minutes");
            unitComboBox.Items.Add("Hours");
            unitComboBox.Items.Add("Days");

            // Parse current interval to set defaults
            ParseIntervalString(_refreshInterval, out double value, out string unit);
            numberBox.Value = value;
            
            // Set unit dropdown
            int unitIndex = unit switch
            {
                "Seconds" => 0,
                "Minutes" => 1,
                "Hours" => 2,
                "Days" => 3,
                _ => 2 // default to Hours
            };
            unitComboBox.SelectedIndex = unitIndex;

            intervalPanel.Children.Add(numberBox);
            intervalPanel.Children.Add(unitComboBox);

            contentPanel.Children.Add(descriptionText);
            contentPanel.Children.Add(intervalPanel);

            dialog.Content = contentPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && unitComboBox.SelectedItem != null && numberBox.Value > 0)
            {
                double intervalValue = numberBox.Value;
                string intervalUnit = unitComboBox.SelectedItem.ToString();
                string selectedInterval = $"{intervalValue} {intervalUnit}";
                
                LogInfo($"New interval set: {selectedInterval}");
                
                // Parse and validate minimum 10 seconds
                var interval = SlideshowService.ParseInterval(selectedInterval);
                
                if (interval.TotalSeconds < 10)
                {
                    LogInfo("Interval too short - minimum is 10 seconds");
                    
                    var errorDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "Invalid Interval",
                        Content = "The interval must be at least 10 seconds.",
                        CloseButtonText = "OK"
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
                
                // Save to class field
                _refreshInterval = selectedInterval;
                
                LogInfo($"Parsed to TimeSpan: {interval}");
                
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

        // Helper method to parse interval string like "12 Hours" or "30 Minutes"
        private void ParseIntervalString(string intervalStr, out double value, out string unit)
        {
            // Default values
            value = 12;
            unit = "Hours";

            if (string.IsNullOrWhiteSpace(intervalStr))
                return;

            var parts = intervalStr.Trim().Split(' ');
            if (parts.Length >= 2)
            {
                if (double.TryParse(parts[0], out double parsedValue))
                {
                    value = parsedValue;
                }
                
                // Capitalize first letter to match ComboBox items
                string unitPart = parts[1].Trim();
                if (!string.IsNullOrEmpty(unitPart))
                {
                    unit = char.ToUpper(unitPart[0]) + unitPart.Substring(1).ToLower();
                }
            }
        }
    }
}
