using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace Wall_You_Need_Next_Gen.Views.Backiee
{
    public sealed partial class SlideshowPage : Page
    {
        public SlideshowPage()
        {
            this.InitializeComponent();
        }

        private void ExpandDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement expand functionality
            System.Diagnostics.Debug.WriteLine("Expand desktop slideshow clicked");
        }

        private void NextDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement next wallpaper functionality
            System.Diagnostics.Debug.WriteLine("Next desktop slideshow clicked");
        }

        private async void EditDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await ShowSlideshowSettingsDialog("Desktop");
        }

        private void ScheduleDesktopSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement schedule functionality
            System.Diagnostics.Debug.WriteLine("Schedule desktop slideshow clicked");
        }

        private void ExpandLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement expand functionality
            System.Diagnostics.Debug.WriteLine("Expand lock screen slideshow clicked");
        }

        private void NextLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement next wallpaper functionality
            System.Diagnostics.Debug.WriteLine("Next lock screen slideshow clicked");
        }

        private async void EditLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            await ShowSlideshowSettingsDialog("Lock Screen");
        }

        private void ScheduleLockScreenSlideshow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement schedule functionality
            System.Diagnostics.Debug.WriteLine("Schedule lock screen slideshow clicked");
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

            var toggleSwitch = new ToggleSwitch
            {
                IsOn = true,
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
            platformComboBox.SelectedIndex = 0;
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
                if (categoryComboBox.Items.Count > 0)
                {
                    categoryComboBox.SelectedIndex = 0;
                }
            };
            
            // Initialize with Backiee categories
            categoryComboBox.Items.Add("Latest Wallpapers");
            categoryComboBox.Items.Add("8K UltraHD");
            categoryComboBox.Items.Add("AI Generated");
            categoryComboBox.SelectedIndex = 0;
            
            contentPanel.Children.Add(categoryComboBox);

            dialog.Content = contentPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Save slideshow settings
                bool isEnabled = toggleSwitch.IsOn;
                string selectedPlatform = platformComboBox.SelectedItem?.ToString() ?? "Backiee";
                string selectedCategory = categoryComboBox.SelectedItem?.ToString() ?? "Latest Wallpapers";

                System.Diagnostics.Debug.WriteLine($"{slideshowType} Slideshow Settings:");
                System.Diagnostics.Debug.WriteLine($"  Enabled: {isEnabled}");
                System.Diagnostics.Debug.WriteLine($"  Platform: {selectedPlatform}");
                System.Diagnostics.Debug.WriteLine($"  Category: {selectedCategory}");

                // Update status text
                if (slideshowType == "Desktop")
                {
                    DesktopStatusText.Text = isEnabled 
                        ? $"{selectedPlatform} - {selectedCategory}" 
                        : "No slideshow set";
                }
                else
                {
                    LockScreenStatusText.Text = isEnabled 
                        ? $"{selectedPlatform} - {selectedCategory}" 
                        : "No slideshow set";
                }
            }
        }
    }
}
