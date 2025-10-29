using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Wall_You_Need_Next_Gen
{
    public partial class App : Application
    {
        private Window? m_window;
        private readonly string logFile = Path.Combine(AppContext.BaseDirectory, "crashlog.txt");

        public App()
        {
            this.InitializeComponent();

            // Catch exceptions on UI and background threads
            UnhandledException += App_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                m_window = new Views.PlatformSelectionWindow();
                m_window.Activate();
            }
            catch (Exception ex)
            {
                LogException("OnLaunched", ex);
                throw;
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            LogException("UI Thread", e.Exception);
            e.Handled = true;
            ShowErrorDialog(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                LogException("Background Thread", ex);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException("Async Task", e.Exception);
            e.SetObserved();
        }

        private void LogException(string source, Exception ex)
        {
            try
            {
                File.AppendAllText(logFile,
                    $"\n[{DateTime.Now}] [{source}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n");
            }
            catch { }
        }

        private async void ShowErrorDialog(Exception ex)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Unexpected Error",
                    Content = $"{ex.Message}\n\n{ex.StackTrace}",
                    CloseButtonText = "OK",
                    XamlRoot = (m_window?.Content as FrameworkElement)?.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch { }
        }
    }
}
