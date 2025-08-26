using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;
using Wall_You_Need_Next_Gen.Models;

namespace Wall_You_Need_Next_Gen.Services
{
    public class AlphaCodersScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly string _smallFolder = "small_thumbs";
        private readonly string _bigFolder = "big_thumbs";
        private readonly string _originalFolder = "originals";
        private readonly string _metadataFolder = "metadata";
        private readonly string _smallJsonFile = "small_urls.json";
        private readonly string _bigJsonFile = "big_urls.json";
        private readonly string _originalJsonFile = "original_urls.json";
        private readonly string _baseUrl = "https://alphacoders.com/resolution/4k-wallpapers?page={0}";

        private readonly List<string> _allSmallUrls = new List<string>();
        private readonly List<string> _allBigUrls = new List<string>();
        private readonly List<string> _allOriginalUrls = new List<string>();

        public AlphaCodersScraperService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            // Create directories
            Directory.CreateDirectory(_smallFolder);
            Directory.CreateDirectory(_bigFolder);
            Directory.CreateDirectory(_originalFolder);
            Directory.CreateDirectory(_metadataFolder);
        }

        public async Task<List<WallpaperItem>> ScrapeWallpapersAsync(int startPage = 1, int endPage = 3)
        {
            var wallpapers = new List<WallpaperItem>();

            try
            {
                // Part 1: Get small image URLs
                Console.WriteLine($"Starting to scrape pages {startPage} to {endPage - 1}...");
                for (int page = startPage; page < endPage; page++)
                {
                    Console.WriteLine($"Scraping page {page}...");
                    var smallUrls = await GetSmallImageUrlsAsync(page);
                    Console.WriteLine($"Found {smallUrls.Count} small images on page {page}");

                    // Log first few URLs for debugging
                    for (int i = 0; i < Math.Min(3, smallUrls.Count); i++)
                    {
                        Console.WriteLine($"  Small URL {i + 1}: {smallUrls[i]}");
                    }

                    _allSmallUrls.AddRange(smallUrls);
                }
                Console.WriteLine($"Total small URLs collected: {_allSmallUrls.Count}");

                // Part 2 & 3: Get big and original image URLs (without downloading)
                Console.WriteLine("Processing URLs to get big and original variants...");
                var semaphore = new SemaphoreSlim(10, 10); // Limit concurrent requests
                var tasks = new List<Task>();

                foreach (var smallUrl in _allSmallUrls)
                {
                    tasks.Add(GetImageUrlsAsync(smallUrl, semaphore));
                }

                await Task.WhenAll(tasks);

                Console.WriteLine($"Extracted {_allBigUrls.Count} big URLs and {_allOriginalUrls.Count} original URLs");

                // Log first few big and original URLs for debugging
                Console.WriteLine("Sample big URLs:");
                for (int i = 0; i < Math.Min(3, _allBigUrls.Count); i++)
                {
                    Console.WriteLine($"  Big URL {i + 1}: {_allBigUrls[i]}");
                }

                Console.WriteLine("Sample original URLs:");
                for (int i = 0; i < Math.Min(3, _allOriginalUrls.Count); i++)
                {
                    Console.WriteLine($"  Original URL {i + 1}: {_allOriginalUrls[i]}");
                }

                // Create WallpaperItem objects
                wallpapers = CreateWallpaperItems();

                return wallpapers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ScrapeWallpapersAsync: {ex.Message}");
                return wallpapers;
            }
        }

        private async Task<List<string>> GetSmallImageUrlsAsync(int pageNumber)
        {
            var imageUrls = new List<string>();

            try
            {
                var url = string.Format(_baseUrl, pageNumber);
                Console.WriteLine($"Fetching URL: {url}");
                var response = await _httpClient.GetAsync(url);
                var htmlContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Got HTML response, length: {htmlContent.Length} characters");

                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var imgNodes = doc.DocumentNode.SelectNodes("//img[@src]");
                Console.WriteLine($"Found {imgNodes?.Count ?? 0} img nodes");

                if (imgNodes != null)
                {
                    int thumbbigCount = 0;
                    foreach (var imgNode in imgNodes)
                    {
                        var src = imgNode.GetAttributeValue("src", "");
                        if (!string.IsNullOrEmpty(src))
                        {
                            if (src.Contains("thumbbig"))
                            {
                                thumbbigCount++;
                                Console.WriteLine($"Found thumbbig image: {src}");
                            }

                            if (src.Contains("thumbbig") && src.StartsWith("https://images"))
                            {
                                imageUrls.Add(src);
                            }
                        }
                    }
                    Console.WriteLine($"Total thumbbig images found: {thumbbigCount}");
                    Console.WriteLine($"Valid thumbbig images (starting with https://images): {imageUrls.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting small image URLs from page {pageNumber}: {ex.Message}");
            }

            return imageUrls;
        }



        private string GetImageIdFromUrl(string url)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(new Uri(url).LocalPath);
                var parts = fileName.Split('-');
                return parts.Last();
            }
            catch
            {
                return "";
            }
        }

        private async Task GetImageUrlsAsync(string smallUrl, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();

            try
            {
                // Get big image URL (without downloading)
                var bigUrl = GetBigImageUrl(smallUrl);
                if (!string.IsNullOrEmpty(bigUrl))
                {
                    lock (_allBigUrls)
                    {
                        _allBigUrls.Add(bigUrl);
                    }
                    Console.WriteLine($"Generated big URL: {bigUrl}");
                }

                // Get original image URL (without downloading)
                var originalUrl = GetOriginalImageUrl(smallUrl);
                if (!string.IsNullOrEmpty(originalUrl))
                {
                    lock (_allOriginalUrls)
                    {
                        _allOriginalUrls.Add(originalUrl);
                    }
                    Console.WriteLine($"Generated original URL: {originalUrl}");
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private string GetBigImageUrl(string smallUrl)
        {
            try
            {
                var imageId = GetImageIdFromUrl(smallUrl);
                var uri = new Uri(smallUrl);
                var domain = uri.Host;
                var folderNumber = uri.Segments[1].TrimEnd('/');

                // Default to jpg extension for big images
                var bigUrl = $"https://{domain}/{folderNumber}/thumb-1920-{imageId}.jpg";
                Console.WriteLine($"Converting small URL: {smallUrl}");
                Console.WriteLine($"  Image ID: {imageId}");
                Console.WriteLine($"  Domain: {domain}");
                Console.WriteLine($"  Folder: {folderNumber}");
                Console.WriteLine($"  Big URL: {bigUrl}");
                return bigUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting big image URL for {smallUrl}: {ex.Message}");
                return null;
            }
        }

        private string GetOriginalImageUrl(string smallUrl)
        {
            try
            {
                var imageId = GetImageIdFromUrl(smallUrl);
                var uri = new Uri(smallUrl);
                var domainParts = uri.Host.Split('.');
                var domainShort = domainParts[0]; // e.g., images3

                // Default to jpg extension for original images
                var originalUrl = $"https://initiate.alphacoders.com/download/{domainShort}/{imageId}/jpg";
                Console.WriteLine($"Converting to original URL:");
                Console.WriteLine($"  Small URL: {smallUrl}");
                Console.WriteLine($"  Image ID: {imageId}");
                Console.WriteLine($"  Domain short: {domainShort}");
                Console.WriteLine($"  Original URL: {originalUrl}");
                return originalUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting original image URL for {smallUrl}: {ex.Message}");
                return null;
            }
        }



        private List<WallpaperItem> CreateWallpaperItems()
        {
            var wallpapers = new List<WallpaperItem>();

            try
            {
                for (int i = 0; i < _allSmallUrls.Count; i++)
                {
                    var smallUrl = _allSmallUrls[i];
                    var imageId = GetImageIdFromUrl(smallUrl);

                    // Find corresponding big and original URLs
                    var bigUrl = _allBigUrls.FirstOrDefault(url => url.Contains(imageId));
                    var originalUrl = _allOriginalUrls.FirstOrDefault(url => url.Contains(imageId));

                    var wallpaper = new WallpaperItem
                    {
                        Id = imageId,
                        Title = $"Alpha Coders Wallpaper {imageId}",
                        ImageUrl = smallUrl, // Small thumb for grid
                        FullPhotoUrl = bigUrl ?? smallUrl, // Big thumb for detail page
                        SourceUrl = originalUrl ?? bigUrl ?? smallUrl, // Original for download
                        Resolution = "3840x2160", // Default 4K
                        QualityTag = "4K",
                        Likes = new Random().Next(10, 1000).ToString(),
                        Downloads = new Random().Next(100, 5000).ToString(),
                        IsAI = false
                    };

                    Console.WriteLine($"Created wallpaper item {i + 1}:");
                    Console.WriteLine($"  ID: {imageId}");
                    Console.WriteLine($"  Small URL: {smallUrl}");
                    Console.WriteLine($"  Big URL: {bigUrl}");
                    Console.WriteLine($"  Original URL: {originalUrl}");

                    wallpapers.Add(wallpaper);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating wallpaper items: {ex.Message}");

                Console.WriteLine($"Total wallpaper items created: {wallpapers.Count}");
            }

            return wallpapers;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
