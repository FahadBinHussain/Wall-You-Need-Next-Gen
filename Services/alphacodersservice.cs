using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Wall_You_Need_Next_Gen.Models;

namespace Wall_You_Need_Next_Gen.Services
{
    public class AlphaCodersService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "your-api-key"; // Replace with your Alpha Coders API key
        private readonly string _apiBaseUrl = "https://wall.alphacoders.com/api2.0/get.php";

        public AlphaCodersService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<WallpaperItem>> GetLatestWallpapersAsync(int page = 1, int count = 30)
        {
            try
            {
                // Build the API request URL
                string requestUrl = $"{_apiBaseUrl}?auth={_apiKey}&method=newest&page={page}&count={count}";
                
                // For demo purposes, return placeholder data
                return GeneratePlaceholderWallpapers(count);
                
                // Uncomment below to use actual API when you have a valid API key
                /*
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    return ParseWallpaperResponse(jsonContent);
                }
                else
                {
                    // Handle API error
                    return new List<WallpaperItem>();
                }
                */
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error fetching wallpapers: {ex.Message}");
                return new List<WallpaperItem>();
            }
        }

        public async Task<WallpaperItem> GetWallpaperDetailsAsync(string wallpaperId)
        {
            try
            {
                // Build the API request URL
                string requestUrl = $"{_apiBaseUrl}?auth={_apiKey}&method=wallpaper_info&id={wallpaperId}";
                
                // For demo purposes, return a placeholder wallpaper
                return GeneratePlaceholderWallpaper(wallpaperId);
                
                // Uncomment below to use actual API when you have a valid API key
                /*
                HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    var wallpapers = ParseWallpaperResponse(jsonContent);
                    return wallpapers.Count > 0 ? wallpapers[0] : null;
                }
                else
                {
                    // Handle API error
                    return null;
                }
                */
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error fetching wallpaper details: {ex.Message}");
                return null;
            }
        }

        private List<WallpaperItem> ParseWallpaperResponse(string jsonContent)
        {
            var wallpapers = new List<WallpaperItem>();
            
            try
            {
                // Parse the JSON response
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    
                    // Check if the API returned success
                    if (root.TryGetProperty("success", out JsonElement success) && success.GetBoolean())
                    {
                        // Get the wallpapers array
                        if (root.TryGetProperty("wallpapers", out JsonElement wallpapersArray))
                        {
                            foreach (JsonElement wallpaperElement in wallpapersArray.EnumerateArray())
                            {
                                var wallpaper = new WallpaperItem
                                {
                                    Id = wallpaperElement.GetProperty("id").GetString(),
                                    Title = wallpaperElement.GetProperty("name").GetString(),
                                    ImageUrl = wallpaperElement.GetProperty("url_thumb").GetString(),
                                    FullPhotoUrl = wallpaperElement.GetProperty("url_image").GetString(),
                                    SourceUrl = wallpaperElement.GetProperty("url_page").GetString(),
                                    Resolution = $"{wallpaperElement.GetProperty("width").GetInt32()}x{wallpaperElement.GetProperty("height").GetInt32()}",
                                    QualityTag = DetermineQualityTag(wallpaperElement.GetProperty("width").GetInt32(), wallpaperElement.GetProperty("height").GetInt32()),
                                    Likes = wallpaperElement.GetProperty("favorites").GetString(),
                                    Downloads = wallpaperElement.GetProperty("views").GetString(),
                                    IsAI = false // Alpha Coders doesn't specify AI-generated content in the API
                                };
                                
                                wallpapers.Add(wallpaper);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing wallpaper response: {ex.Message}");
            }
            
            return wallpapers;
        }

        private string DetermineQualityTag(int width, int height)
        {
            int maxDimension = Math.Max(width, height);
            
            if (maxDimension >= 7680) return "8K";
            if (maxDimension >= 5120) return "5K";
            if (maxDimension >= 3840) return "4K";
            if (maxDimension >= 2560) return "2K";
            
            return string.Empty;
        }

        private List<WallpaperItem> GeneratePlaceholderWallpapers(int count)
        {
            var wallpapers = new List<WallpaperItem>();
            
            for (int i = 1; i <= count; i++)
            {
                wallpapers.Add(new WallpaperItem
                {
                    Id = i.ToString(),
                    Title = $"Alpha Coders Wallpaper {i}",
                    ImageUrl = "ms-appx:///Assets/placeholder-wallpaper-1000.jpg",
                    FullPhotoUrl = "ms-appx:///Assets/placeholder-wallpaper-1000.jpg",
                    SourceUrl = $"https://wall.alphacoders.com/wallpaper.php?i={i}",
                    Resolution = "3840x2160",
                    QualityTag = "4K",
                    Likes = new Random().Next(10, 1000).ToString(),
                    Downloads = new Random().Next(100, 5000).ToString(),
                    IsAI = i % 3 == 0 // Every third wallpaper is AI-generated for demo purposes
                });
            }
            
            return wallpapers;
        }

        private WallpaperItem GeneratePlaceholderWallpaper(string id)
        {
            int idNumber = int.TryParse(id, out int result) ? result : 1;
            
            return new WallpaperItem
            {
                Id = id,
                Title = $"Alpha Coders Wallpaper {id}",
                ImageUrl = "ms-appx:///Assets/placeholder-wallpaper-1000.jpg",
                FullPhotoUrl = "ms-appx:///Assets/placeholder-wallpaper-1000.jpg",
                SourceUrl = $"https://wall.alphacoders.com/wallpaper.php?i={id}",
                Resolution = "3840x2160",
                QualityTag = "4K",
                Likes = new Random().Next(10, 1000).ToString(),
                Downloads = new Random().Next(100, 5000).ToString(),
                IsAI = idNumber % 3 == 0 // Every third wallpaper is AI-generated for demo purposes
            };
        }
    }
}