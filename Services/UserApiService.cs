using System.Net.Http.Headers;
using System.Text.Json;
using courses_platform.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;

namespace courses_platform.Services
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(string userId)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync($"https://localhost:5000/api/users/{userId}");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<UserProfileViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<UserProfileViewModel?> UpdateUserProfileAsync(string userId, UserProfileUpdateModel model)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.FullName ?? ""), "FullName");
            if (!string.IsNullOrEmpty(model.Bio))
                content.Add(new StringContent(model.Bio), "Bio");
            if (!string.IsNullOrEmpty(model.SocialLinks))
                content.Add(new StringContent(model.SocialLinks), "SocialLinks");
            if (model.PhotoFile != null)
                content.Add(new StreamContent(model.PhotoFile.OpenReadStream()), "PhotoFile", model.PhotoFile.FileName);

            var response = await _httpClient.PutAsync($"https://localhost:5000/api/users/{userId}", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public class UserProfileUpdateModel
        {
            public string FullName { get; set; } = null!;
            public string? Bio { get; set; }
            public string? SocialLinks { get; set; }
            public IFormFile? PhotoFile { get; set; }
        }
    }
}
