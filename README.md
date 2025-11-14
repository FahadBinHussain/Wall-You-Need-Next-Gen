# Wall-You-Need: Next Generation
<img src="https://wakapi-qt1b.onrender.com/api/badge/fahad/interval:any/project:Wall-You-Need-Next-Gen" 
     alt="Wakapi Time Tracking" 
     title="Spent more than that amount of time spent on this project">

A modern wallpaper management and personalization application built with WinUI 3, delivering a seamless user experience for browsing, organizing, and applying beautiful wallpapers to your Windows desktop.

![Wall-You-Need App](https://placeholder-for-app-screenshot.png)

## 🌟 Features

- **Extensive Wallpaper Collection**: Browse through a vast library of high-quality wallpapers
- **Intelligent Categorization**: Navigate wallpapers by collections, AI-generated content, and more
- **Infinite Scrolling**: Enjoy a smooth browsing experience with dynamic content loading
- **Adaptive Layout**: Responsive grid that adapts to any screen size and resolution
- **Visual Tagging System**: Easily identify wallpaper qualities (4K, 5K, 8K) and AI-generated content
- **Personalization Options**: Create and manage custom collections of your favorite wallpapers
- **Slideshow Functionality**: Set multiple wallpapers to rotate automatically
- **Interactive Slideshow**: Create dynamic, interactive wallpaper experiences
- **Modern UI**: Clean, intuitive interface following Fluent Design principles
- **Performance Optimized**: Fast loading and smooth scrolling with virtualized content

## 🚀 Getting Started

### Prerequisites

- Windows 10 version 1809 or higher
- [Windows App SDK 1.2 or higher](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with the following workloads:
  - Universal Windows Platform development
  - .NET Desktop Development

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/Wall-You-Need-Next-Gen.git
   ```

2. Open the solution in Visual Studio 2022
3. Restore NuGet packages
4. Build and run the application

# Run Directly
1. dotnet run

# Build the application
### Publish as a self-contained executable
1. dotnet publish -c Release -p:Platform=x64 -r win-x64 --self-contained true
2. check C:\Users\Admin\Downloads\Wall-You-Need-Next-Gen\bin\x64\Release\win-x64\publish (This folder is portable and can be copied to any location to run the app)

### Publish as a Single EXE, No MSIX, No installer, Self-contained, Works on any Windows 10+ machine
1. dotnet publish -c Release -p:Platform=x64 -r win-x64 --self-contained true ` -p:PublishSingleFile=true ` -p:IncludeAllContentForSelfExtract=true ` -p:EnableCompressionInSingleFile=true ` -p:WindowsAppSDKSelfContained=true ` -p:UseWinUI=true
2. check C:\Users\Admin\Downloads\Wall-You-Need-Next-Gen\bin\x64\Release\win-x64\publish\Wall-You-Need-Next-Gen.exe

## 📁 Project Architecture

Wall-You-Need follows a modern MVVM architecture pattern:

- **Views**: XAML-based UI components with minimal code-behind
- **Models**: Data structures and business logic
- **Services**: API integrations and system interactions
- **Navigation**: Centralized navigation system with persistent state

### Key Components

- **Main Window**: Custom title bar, navigation sidebar, and content frame
- **Navigation Panel**: Two-tier navigation with main items and footer items
- **Content Pages**: Specialized pages for different functionality
- **API Integration**: Remote wallpaper service for browsing and downloading
- **ProgressBar**: Non-intrusive loading indicators for asynchronous operations

## 🧩 Technologies

- **WinUI 3**: Modern native UI framework 
- **Windows App SDK**: Building Windows apps with a unified set of APIs and tools
- **C# 10**: Modern language features for clean, maintainable code
- **XAML**: Declarative UI definition with styling and data binding
- **JSON**: Lightweight data interchange with the wallpaper API
- **HTTP Client**: Modern API communication
- **Virtualization**: Efficient rendering of large collections

## 🔍 Code Highlights

### Responsive UI

The application employs a responsive grid system to display wallpapers, automatically adjusting to window size changes:

```csharp
private void WallpapersWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
{
    if (sender is ItemsWrapGrid wrapGrid)
    {
        // Calculate columns based on available width
        double availableWidth = e.NewSize.Width;
        double desiredItemWidth = 300;
        double itemMargin = 8;
        
        int columnsCount = Math.Max(1, (int)(availableWidth / desiredItemWidth));
        columnsCount = Math.Min(columnsCount, 6);
        
        wrapGrid.MaximumRowsOrColumns = columnsCount;
        
        // Calculate proportional item dimensions
        double totalMarginWidth = (columnsCount - 1) * itemMargin;
        double newItemWidth = (availableWidth - totalMarginWidth) / columnsCount;
        double finalWidth = Math.Max(200, newItemWidth);
        double aspectRatio = 16.0 / 9.0;
        double finalHeight = finalWidth / aspectRatio;
        
        wrapGrid.ItemWidth = finalWidth;
        wrapGrid.ItemHeight = finalHeight;
    }
}
```

### Infinite Scrolling

The application implements a smooth infinite scrolling mechanism to load content as the user scrolls:

```csharp
private async void MainScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
{
    if (sender is ScrollViewer scrollViewer)
    {
        double verticalOffset = scrollViewer.VerticalOffset;
        double maxVerticalOffset = scrollViewer.ScrollableHeight;
        
        if (maxVerticalOffset > 0 &&
            verticalOffset >= maxVerticalOffset * _loadMoreThreshold &&
            !_isLoading)
        {
            await LoadMoreWallpapers();
        }
    }
}
```

## 📈 Roadmap

- **Widget Support**: Desktop widgets for wallpaper previews and controls
- **User Accounts**: Cloud synchronization of collections and preferences
- **AI Generation**: Generate custom wallpapers using AI
- **Enhanced Search**: Advanced filtering and search capabilities
- **Performance Optimizations**: Further improvements to loading and rendering

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 🙏 Acknowledgments

- Wallpaper API provided by [Backiee](https://backiee.com/)
- Icons from [Fluent UI System Icons](https://github.com/microsoft/fluentui-system-icons)
- WinUI 3 and Windows App SDK teams at Microsoft

---

Built with ❤️ using WinUI 3 and Windows App SDK
