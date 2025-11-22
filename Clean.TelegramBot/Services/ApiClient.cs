using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Clean.TelegramBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Clean.TelegramBot.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_configuration["Api:BaseUrl"]!);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ApiResponse<LoginResponse>?> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest { Name = username, UserPassword = password };
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Attempting login for user: {Username} to {BaseUrl}/auth/login", username, _httpClient.BaseAddress);
            
            var response = await _httpClient.PostAsync("/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Login response status: {StatusCode}, content: {Content}", response.StatusCode, responseContent);

            // Try to parse the response even if status code is not success
            var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Login failed with status {StatusCode}: {Content}", response.StatusCode, responseContent);
                // Return the error response so we can show the actual error message
                return result ?? new ApiResponse<LoginResponse> 
                { 
                    StatusCode = (int)response.StatusCode, 
                    Message = $"HTTP {(int)response.StatusCode}: {responseContent}" 
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login. Is the backend API running at {BaseUrl}?", _httpClient.BaseAddress);
            return new ApiResponse<LoginResponse>
            {
                StatusCode = 500,
                Message = $"Connection error: {ex.Message}. Make sure your backend API is running."
            };
        }
    }

    public async Task<ApiResponse<LoginResponse>?> LoginByTelegramAsync(long chatId)
    {
        try
        {
            var request = new TelegramLoginRequest { ChatId = chatId };
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Attempting Telegram login for ChatId: {ChatId} to {BaseUrl}/auth/login/telegram", chatId, _httpClient.BaseAddress);
            
            var response = await _httpClient.PostAsync("/auth/login/telegram", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Telegram login response status: {StatusCode}, content: {Content}", response.StatusCode, responseContent);

            var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Telegram login failed with status {StatusCode}: {Content}", response.StatusCode, responseContent);
                return result ?? new ApiResponse<LoginResponse> 
                { 
                    StatusCode = (int)response.StatusCode, 
                    Message = $"HTTP {(int)response.StatusCode}: {responseContent}" 
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Telegram login. Is the backend API running at {BaseUrl}?", _httpClient.BaseAddress);
            return new ApiResponse<LoginResponse>
            {
                StatusCode = 500,
                Message = $"Connection error: {ex.Message}. Make sure your backend API is running."
            };
        }
    }

    public async Task<bool> LinkTelegramAccountAsync(string username, long chatId)
    {
        try
        {
            var request = new { Username = username, ChatId = chatId };
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/auth/link-telegram", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking Telegram account");
            return false;
        }
    }

    public async Task<PagedResponse<NoteGetDto>?> GetNotesAsync(string token, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/notes/get-all?PageNumber={pageNumber}&PageSize={pageSize}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Get notes failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }

            var result = JsonSerializer.Deserialize<PagedResponse<NoteGetDto>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes");
            return null;
        }
    }

    public async Task<ApiResponse<NoteGetDto>?> GetNoteByIdAsync(string token, int id)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/notes/{id}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Get note failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }

            var result = JsonSerializer.Deserialize<ApiResponse<NoteGetDto>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting note by id");
            return null;
        }
    }

    public async Task<ApiResponse<NoteGetDto>?> CreateNoteAsync(string token, NoteCreateDto note)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = JsonSerializer.Serialize(note, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/notes/create", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Create note failed with status {StatusCode}: {Content}", response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<ApiResponse<NoteGetDto>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note");
            return null;
        }
    }

    public async Task<ApiResponse<NoteGetDto>?> UpdateNoteAsync(string token, NoteUpdateDto note)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = JsonSerializer.Serialize(note, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("/notes/update", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Update note failed with status {StatusCode}: {Content}", response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<ApiResponse<NoteGetDto>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating note");
            return null;
        }
    }

    public async Task<bool> DeleteNoteAsync(string token, int id)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"/notes/{id}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting note");
            return false;
        }
    }

    public async Task<PagedResponse<ReminderGetDto>?> GetRemindersAsync(string token, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/reminder/get-all?PageNumber={pageNumber}&PageSize={pageSize}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Get reminders failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }

            var result = JsonSerializer.Deserialize<PagedResponse<ReminderGetDto>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminders");
            return null;
        }
    }

    public async Task<ApiResponse<ReminderGetDto>?> GetReminderByIdAsync(string token, int id)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"/reminder/{id}");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Get reminder failed with status {StatusCode}: {Content}", response.StatusCode, content);
                return null;
            }

            var result = JsonSerializer.Deserialize<ApiResponse<ReminderGetDto>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reminder by id");
            return null;
        }
    }

    public async Task<ApiResponse<ReminderGetDto>?> CreateReminderAsync(string token, ReminderCreateDto reminder)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = JsonSerializer.Serialize(reminder, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/reminder/create", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Create reminder failed with status {StatusCode}: {Content}", response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<ApiResponse<ReminderGetDto>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reminder");
            return null;
        }
    }

    public async Task<ApiResponse<ReminderGetDto>?> UpdateReminderAsync(string token, ReminderUpdateDto reminder)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var json = JsonSerializer.Serialize(reminder, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("/reminder/update", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Update reminder failed with status {StatusCode}: {Content}", response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<ApiResponse<ReminderGetDto>>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reminder");
            return null;
        }
    }

    public async Task<bool> DeleteReminderAsync(string token, int id)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"/reminder/{id}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reminder");
            return false;
        }
    }
}

