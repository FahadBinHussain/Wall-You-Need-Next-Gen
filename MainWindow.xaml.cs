using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Wall_You_Need_Next_Gen.Views;
using WinRT.Interop;
using Windows.Storage;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Wall_You_Need_Next_Gen
{
    /// <summary>
    /// Main application window with navigation.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow m_appWindow;
        
        // Add a flag to track if we're already handling a resize operation
        private bool isHandlingResize = false;
        private SizeInt32 lastAppliedSize;
        
        public MainWindow()
        {
            this.InitializeComponent();
            
            // Set up custom titlebar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(CustomTitleBar);
            
            // Ensure the navbar selected brush is correctly initialized
            try
            {
                // Initialize the selected background brush with the desired color
                var selectedBgBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 234, 234, 234)); // #eaeaea
                
                // We need to access the resource dictionary through Content's Resources in WinUI 3
                if (Content is FrameworkElement element)
                {
                    // Try to add or replace the resource in the content's resource dictionary
                    if (element.Resources.TryGetValue("NavbarSelectedBgBrush", out _))
                    {
                        element.Resources["NavbarSelectedBgBrush"] = selectedBgBrush;
                    }
                    else
                    {
                        element.Resources.Add("NavbarSelectedBgBrush", selectedBgBrush);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any error but don't crash
                System.Diagnostics.Debug.WriteLine($"Failed to initialize selected brush: {ex.Message}");
            }
            
            // Change the window's title
            Title = "Wall-You-Need";
            
            // Get the AppWindow for this window
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            m_appWindow = AppWindow.GetFromWindowId(windowId);
            
            // Initialize the last applied size with default minimum values
            lastAppliedSize = new SizeInt32(800, 600);
            
            // Set initial window size if needed - without triggering resize event
            if (m_appWindow.Size.Width < 800 || m_appWindow.Size.Height < 600)
            {
                isHandlingResize = true;
                try
                {
                    m_appWindow.Resize(new SizeInt32(
                        Math.Max(m_appWindow.Size.Width, 800),
                        Math.Max(m_appWindow.Size.Height, 600)));
                }
                finally
                {
                    isHandlingResize = false;
                }
            }
            
            // Register for window closing event
            m_appWindow.Closing += AppWindow_Closing;
            
            // Register for window resizing event
            m_appWindow.Changed += AppWindow_Changed;
            
            // Restore window position and size if available
            RestoreWindowPositionAndSize();
            
            // Navigate to the homepage by default
            ContentFrame.Navigate(typeof(HomePage));
            
            // Apply the correct style to the default selected Home button
            if (HomeButton != null)
            {
                // Reset all buttons to ensure clean state
                ResetAllNavButtonStyles();
                
                // Apply the selected style with the correct text color to Home button
                ApplySelectedButtonStyle(HomeButton);
            }
        }

        private void ResetAllNavButtonStyles()
        {
            // Reset all navigation buttons in main nav panel
            ResetButtonsInNavPanel(NavPanel);
            
            // Reset footer buttons - do this by finding all buttons with MyAccount or Settings tags
            ResetSpecificButtons("MyAccount", "Settings");
        }
        
        private void ResetButtonsInNavPanel(StackPanel panel)
        {
            if (panel == null) return;
            
            foreach (var child in panel.Children)
            {
                if (child is Button button)
                {
                    try
                    {
                        ResetButtonStyle(button);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue
                        System.Diagnostics.Debug.WriteLine($"Error resetting button style: {ex.Message}");
                    }
                }
            }
        }
        
        private void ResetSpecificButtons(params string[] buttonTags)
        {
            // Find and reset specific buttons by their tag values
            try
            {
                if (MyAccountButton != null)
                    ResetButtonStyle(MyAccountButton);
                    
                if (SettingsButton != null)
                    ResetButtonStyle(SettingsButton);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting specific buttons: {ex.Message}");
            }
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            // Save window position and size when closing
            SaveWindowPositionAndSize();
        }
        
        private void SaveWindowPositionAndSize()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            // Save position
            localSettings.Values["WindowPositionX"] = m_appWindow.Position.X;
            localSettings.Values["WindowPositionY"] = m_appWindow.Position.Y;
            
            // Save size
            localSettings.Values["WindowWidth"] = m_appWindow.Size.Width;
            localSettings.Values["WindowHeight"] = m_appWindow.Size.Height;
        }
        
        private void RestoreWindowPositionAndSize()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            // Check if we have saved position and size
            if (localSettings.Values.ContainsKey("WindowPositionX") &&
                localSettings.Values.ContainsKey("WindowPositionY") &&
                localSettings.Values.ContainsKey("WindowWidth") &&
                localSettings.Values.ContainsKey("WindowHeight"))
            {
                try
                {
                    // Temporarily disable resize handling during restoration
                    isHandlingResize = true;
                    
                    try
                    {
                        // Get display information to validate window position
                        // For simplicity, we'll just ensure window is not positioned negatively
                        
                        // Restore position with validation
                        int posX = (int)localSettings.Values["WindowPositionX"];
                        int posY = (int)localSettings.Values["WindowPositionY"];
                        
                        // Ensure window is not positioned off-screen
                        posX = Math.Max(posX, 0);
                        posY = Math.Max(posY, 0);
                        
                        // Apply validated position
                        m_appWindow.Move(new PointInt32(posX, posY));
                        
                        // Restore size with validation
                        int width = (int)localSettings.Values["WindowWidth"];
                        int height = (int)localSettings.Values["WindowHeight"];
                        
                        // Ensure window is not too small
                        width = Math.Max(width, 800);
                        height = Math.Max(height, 600);
                        
                        // Update last applied size
                        lastAppliedSize = new SizeInt32(width, height);
                        
                        // Temporarily unsubscribe from resize events
                        m_appWindow.Changed -= AppWindow_Changed;
                        
                        // Apply validated size
                        m_appWindow.Resize(lastAppliedSize);
                    }
                    finally
                    {
                        // Resubscribe to resize events
                        m_appWindow.Changed += AppWindow_Changed;
                        isHandlingResize = false;
                    }
                }
                catch (Exception ex)
                {
                    // If restoration fails, set to default size
                    try
                    {
                        lastAppliedSize = new SizeInt32(1024, 768);
                        m_appWindow.Changed -= AppWindow_Changed;
                        m_appWindow.Resize(lastAppliedSize);
                        m_appWindow.Changed += AppWindow_Changed;
                    }
                    catch
                    {
                        // Last resort - ignore if even this fails
                    }
                }
            }
            else
            {
                // No saved settings, use default size
                try
                {
                    lastAppliedSize = new SizeInt32(1024, 768);
                    m_appWindow.Changed -= AppWindow_Changed;
                    m_appWindow.Resize(lastAppliedSize);
                    m_appWindow.Changed += AppWindow_Changed;
                }
                catch
                {
                    // Ignore errors
                }
            }
        }

        // Add a handler for window resizing to enforce minimum size
        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            // Only handle size changes
            if (args.DidSizeChange && !isHandlingResize)
            {
                // Get current size
                var currentSize = m_appWindow.Size;
                
                // Check if resize is needed
                int newWidth = Math.Max(currentSize.Width, 800);
                int newHeight = Math.Max(currentSize.Height, 600);
                
                // Only resize if necessary and different from last applied size
                if ((newWidth != currentSize.Width || newHeight != currentSize.Height) &&
                    (newWidth != lastAppliedSize.Width || newHeight != lastAppliedSize.Height))
                {
                    // Set flag to prevent reentrancy and remember this size
                    isHandlingResize = true;
                    lastAppliedSize = new SizeInt32(newWidth, newHeight);
                    
                    try
                    {
                        // Temporarily unsubscribe to prevent events while changing size
                        m_appWindow.Changed -= AppWindow_Changed;
                        m_appWindow.Resize(lastAppliedSize);
                    }
                    finally
                    {
                        // Always resubscribe and clear flag
                        m_appWindow.Changed += AppWindow_Changed;
                        isHandlingResize = false;
                    }
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                string navItemTag = args.SelectedItemContainer.Tag.ToString();
                
                // Navigation logic based on the selected tag
                switch (navItemTag)
                {
                    case "Home":
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                    case "Collections":
                    case "AIGenerated":
                    case "Personal":
                    case "Slideshow":
                    case "InteractiveSlideshow":
                    case "Widgets":
                    case "UploadWallpaper":
                    case "MyAccount":
                    case "Settings":
                        // For now, we'll just navigate to Home for all options
                        // In a real app, you would navigate to different pages
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                }
            }
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    string navItemTag = button.Tag?.ToString();
                    if (string.IsNullOrEmpty(navItemTag))
                        return;
                    
                    // Reset all nav buttons to their default style
                    ResetAllNavButtonStyles();
                    
                    // Apply selected style to the clicked button
                    ApplySelectedButtonStyle(button);
                    
                    // Navigate based on the tag
                    switch (navItemTag)
                    {
                        case "Home":
                            ContentFrame.Navigate(typeof(HomePage));
                            break;
                        case "Collections":
                        case "AIGenerated":
                        case "Personal":
                        case "Slideshow":
                        case "InteractiveSlideshow":
                        case "Widgets":
                        case "UploadWallpaper":
                        case "MyAccount":
                        case "Settings":
                            // For now, navigate to Home page for all options
                            // In a real app, you would navigate to the appropriate page
                            ContentFrame.Navigate(typeof(HomePage));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't crash
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
            }
        }

        private void ResetButtonStyle(Button navButton)
        {
            // Get the stack panel inside the button
            if (navButton?.Content is not StackPanel buttonStack || buttonStack.Children.Count == 0)
                return;
                
            // Check if we're using the new Grid-based approach or the old Border approach
            if (buttonStack.Children[0] is Grid buttonGrid)
            {
                // Hide the background and orange indicator for inactive buttons
                foreach (var child in buttonGrid.Children)
                {
                    if (child is Border border)
                    {
                        border.Background = new SolidColorBrush(Colors.Transparent);
                    }
                    else if (child is Microsoft.UI.Xaml.Shapes.Rectangle rectangle)
                    {
                        rectangle.Visibility = Visibility.Collapsed;
                    }
                    else if (child is StackPanel contentPanel)
                    {
                        // Reset text color in the content panel
                        foreach (var contentChild in contentPanel.Children)
                        {
                            if (contentChild is TextBlock textBlock)
                            {
                                textBlock.Foreground = new SolidColorBrush(Colors.DarkGray);
                            }
                        }
                    }
                }
            }
            else if (buttonStack.Children[0] is Border buttonBorder)
            {
                // Old approach - reset border style
                bool isFooter = navButton.Tag?.ToString() == "MyAccount" || navButton.Tag?.ToString() == "Settings";
                
                // Apply appropriate style based on whether it's a footer button or not
                Style defaultStyle = null;
                try
                {
                    if (isFooter)
                    {
                        defaultStyle = Application.Current.Resources["FooterNavItemBorderStyle"] as Style;
                    }
                    else
                    {
                        defaultStyle = Application.Current.Resources["NavItemBorderStyle"] as Style;
                    }
                    
                    // Apply the style if we found it
                    if (defaultStyle != null)
                    {
                        buttonBorder.Style = defaultStyle;
                    }
                }
                catch
                {
                    // If we can't get the style, just set some default properties directly
                    buttonBorder.Background = new SolidColorBrush(Colors.Transparent);
                    buttonBorder.BorderThickness = new Thickness(0);
                }
                
                // Reset text color
                FindAndUpdateTextBlock(buttonStack, new SolidColorBrush(Colors.DarkGray));
            }
        }

        private void ApplySelectedButtonStyle(Button button)
        {
            if (button?.Content is not StackPanel selectedStack || selectedStack.Children.Count == 0)
                return;
            
            // Check if we're using the new Grid-based approach or the old Border approach
            if (selectedStack.Children[0] is Grid selectedGrid)
            {
                // Set up the background and orange indicator for active buttons
                foreach (var child in selectedGrid.Children)
                {
                    if (child is Border border)
                    {
                        border.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 234, 234, 234)); // #eaeaea
                    }
                    else if (child is Microsoft.UI.Xaml.Shapes.Rectangle rectangle)
                    {
                        rectangle.Visibility = Visibility.Visible;
                        rectangle.Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 87, 51)); // #FF5733
                    }
                    else if (child is StackPanel contentPanel)
                    {
                        // Set selected text color in the content panel
                        foreach (var contentChild in contentPanel.Children)
                        {
                            if (contentChild is TextBlock textBlock)
                            {
                                // Try to get the brush from resources first
                                SolidColorBrush textBrush;
                                try 
                                {
                                    textBrush = Application.Current.Resources["NavbarSelectedTextBrush"] as SolidColorBrush;
                                }
                                catch
                                {
                                    // Fall back to creating the brush directly
                                    textBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 25, 25, 25)); // #191919
                                }
                                
                                // If we couldn't get the brush from resources or caught an exception, create it directly
                                if (textBrush == null)
                                {
                                    textBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 25, 25, 25)); // #191919
                                }
                                
                                textBlock.Foreground = textBrush;
                            }
                        }
                    }
                }
            }
            else if (selectedStack.Children[0] is Border selectedBorder)
            {
                // Old approach
                // Apply active style to border
                // Always directly set the background color to ensure consistency
                selectedBorder.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 234, 234, 234)); // #eaeaea
                selectedBorder.BorderThickness = new Thickness(4, 0, 0, 0);
                selectedBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 87, 51)); // #FF5733
                
                // Try to get the style from different resource dictionaries
                Style activeStyle = null;
                
                try
                {
                    // Check if this is a footer button
                    bool isFooter = button.Tag?.ToString() == "MyAccount" || button.Tag?.ToString() == "Settings";
                    
                    // Apply appropriate style based on whether it's a footer button or not
                    if (isFooter)
                    {
                        activeStyle = Application.Current.Resources["FooterNavItemActiveBorderStyle"] as Style;
                        
                        // Make sure to set the height for footer items
                        selectedBorder.Height = 60;
                    }
                    else
                    {
                        activeStyle = Application.Current.Resources["NavItemActiveBorderStyle"] as Style;
                        
                        // Make sure to set the height for regular items
                        selectedBorder.Height = 76;
                    }
                    
                    // Apply the style if we found it
                    if (activeStyle != null)
                    {
                        selectedBorder.Style = activeStyle;
                        
                        // Ensure the background is set to our desired color even after applying the style
                        selectedBorder.Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 234, 234, 234)); // #eaeaea
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue
                    System.Diagnostics.Debug.WriteLine($"Failed to apply active style: {ex.Message}");
                }
                
                // Update selected text color
                SolidColorBrush selectedTextBrush;
                try 
                {
                    // Try to get the brush from resources first
                    selectedTextBrush = Application.Current.Resources["NavbarSelectedTextBrush"] as SolidColorBrush;
                }
                catch
                {
                    // Fall back to creating the brush directly
                    selectedTextBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 25, 25, 25)); // #191919
                }
                
                // If we couldn't get the brush from resources or caught an exception, create it directly
                if (selectedTextBrush == null)
                {
                    selectedTextBrush = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 25, 25, 25)); // #191919
                }
                
                FindAndUpdateTextBlock(selectedStack, selectedTextBrush);
            }
        }

        private void FindAndUpdateTextBlock(StackPanel stack, Brush foreground)
        {
            if (stack == null || foreground == null)
                return;
                
            TextBlock textBlock = null;
            
            // Try to find the TextBlock directly in the stack
            if (stack.Children.Count > 1 && stack.Children[1] is TextBlock tb1)
            {
                textBlock = tb1;
            }
            // Or look inside a Border if present
            else if (stack.Children.Count > 0 && stack.Children[0] is Border border && 
                     border.Child is StackPanel innerStack && 
                     innerStack.Children.Count > 1 && 
                     innerStack.Children[1] is TextBlock tb2)
            {
                textBlock = tb2;
            }
            
            // Update the foreground if we found a TextBlock
            if (textBlock != null)
            {
                textBlock.Foreground = foreground;
            }
        }
    }
}
