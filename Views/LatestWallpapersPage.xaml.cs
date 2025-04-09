using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Wall_You_Need_Next_Gen.Views
{
    public sealed partial class LatestWallpapersPage : Page
    {
        public LatestWallpapersPage()
        {
            this.InitializeComponent();
        }

        private void ClickMeButton_Click(object sender, RoutedEventArgs e)
        {
            // Simple click handler
            ClickMeButton.Content = "Button Clicked!";
        }
    }
} 