using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Clean.Infrastructure.Services;

public class TelegramInitDataVerifier
{
    public class TelegramUser
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? LanguageCode { get; set; }
    }

    public static (bool IsValid, TelegramUser? User) VerifyAndParse(string initData, string botToken)
    {
        try
        {
            // Parse initData (URL-encoded query string)
            var parameters = new Dictionary<string, string>();
            var hash = "";
            
            foreach (var pair in initData.Split('&'))
            {
                var parts = pair.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = Uri.UnescapeDataString(parts[1]);
                    
                    if (key == "hash")
                    {
                        hash = value;
                    }
                    else
                    {
                        parameters[key] = value;
                    }
                }
            }

            if (string.IsNullOrEmpty(hash))
                return (false, null);

            // Reconstruct data string for verification
            var dataCheckString = string.Join("\n", parameters.Keys
                .OrderBy(k => k)
                .Select(k => $"{k}={parameters[k]}"));

            // Compute secret key
            var secretKey = ComputeHmacSha256(botToken, "WebAppData");

            // Compute hash
            var computedHash = ComputeHmacSha256(dataCheckString, secretKey);

            // Verify hash
            if (computedHash != hash)
                return (false, null);

            // Parse user data
            if (!parameters.TryGetValue("user", out var userStr) || string.IsNullOrEmpty(userStr))
                return (false, null);

            var user = JsonSerializer.Deserialize<TelegramUser>(userStr, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return (true, user);
        }
        catch
        {
            return (false, null);
        }
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}

