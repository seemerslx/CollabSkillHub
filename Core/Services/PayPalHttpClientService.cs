using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Core.Services;

public class PayPalHttpClientService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private const string PAYPAL_ACCESS_TOKEN = "PAYPAL_ACCESS_TOKEN";
    private const string PAYPAL_ACCESS_TOKEN_EXPIRY = "PAYPAL_ACCESS_TOKEN_EXPIRY";

    public PayPalHttpClientService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _configuration = configuration;
    }

    public async Task<string> GetAccessTokenAsync(bool fetchNewToken = false)
    {
        if (!fetchNewToken)
        {
            // Attempt to get cached token and its expiry time
            if (_memoryCache.TryGetValue(PAYPAL_ACCESS_TOKEN, out string cachedToken) &&
                _memoryCache.TryGetValue(PAYPAL_ACCESS_TOKEN_EXPIRY, out DateTime expiry))
            {
                if (expiry > DateTime.UtcNow)
                {
                    return cachedToken;
                }
            }
        }

        bool isTestMode = true;

        string clientId = isTestMode
            ? _configuration["PayPal:TestClientId"]
            : _configuration["PayPal:ClientId"];

        string clientSecret = isTestMode
            ? _configuration["PayPal:TestClientSecret"]
            : _configuration["PayPal:ClientSecret"];

        string tokenUrl = isTestMode
            ? _configuration["PayPal:TestTokenUrl"]
            : _configuration["PayPal:TokenUrl"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tokenUrl))
        {
            throw new InvalidOperationException("PayPal configuration is incomplete");
        }

        var client = _httpClientFactory.CreateClient();

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

        try
        {
            var response = await client.PostAsync(tokenUrl, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<PayPalAccessTokenResponse>(responseBody);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return null;
            }

            string accessToken = tokenResponse.AccessToken;
            int expiresIn = tokenResponse.ExpiresIn;

            // Subtract a small buffer (e.g., 600 seconds = 10 minutes) to avoid edge-case expiration
            DateTime expiry = DateTime.UtcNow.AddSeconds(expiresIn - 600);

            // Cache the token and its expiration
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiry);

            _memoryCache.Set(PAYPAL_ACCESS_TOKEN, accessToken, cacheOptions);
            _memoryCache.Set(PAYPAL_ACCESS_TOKEN_EXPIRY, expiry, cacheOptions);

            return accessToken;
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error getting PayPal access token: {ex.Message}");
            return null;
        }
    }

    public async Task<string> PostJsonAsync(string url, string jsonBody, string accessToken)
    {
        var response = await AttemptHttpPostAsync(url, jsonBody, accessToken);

        if (response.statusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var newToken = await GetAccessTokenAsync(true);
            if (!string.IsNullOrEmpty(newToken) && newToken != accessToken)
            {
                // Log retry attempt
                Console.WriteLine($"PayPal POST Retry Request - URL: {url}, TOKEN: {newToken}");
                response = await AttemptHttpPostAsync(url, jsonBody, newToken);
                Console.WriteLine($"PayPal POST Retry Response - StatusCode: {response.statusCode}, ResponseBody: {response.responseBody?.Substring(0, Math.Min(100, response.responseBody?.Length ?? 0))}");
            }
        }

        return response.responseBody;
    }

    public async Task<string> GetJsonAsync(string url, string accessToken)
    {
        var response = await AttemptHttpGetAsync(url, accessToken);

        if (response.statusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var newToken = await GetAccessTokenAsync(true);
            if (!string.IsNullOrEmpty(newToken) && newToken != accessToken)
            {
                // Log retry attempt
                Console.WriteLine($"PayPal GET Retry Request - URL: {url}, TOKEN: {newToken}");
                response = await AttemptHttpGetAsync(url, newToken);
                Console.WriteLine($"PayPal GET Retry Response - StatusCode: {response.statusCode}, ResponseBody: {response.responseBody?.Substring(0, Math.Min(100, response.responseBody?.Length ?? 0))}");
            }
        }

        return response.responseBody;
    }

    private async Task<(System.Net.HttpStatusCode statusCode, string responseBody)> AttemptHttpPostAsync(
        string url, string jsonBody, string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(url, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            return (response.StatusCode, responseBody);
        }
        catch (HttpRequestException ex)
        {
            // Log the exception
            Console.WriteLine($"ERROR PayPalHttpClientService / POST - {url} | {ex.Message}");
            return (System.Net.HttpStatusCode.InternalServerError, null);
        }
    }

    private async Task<(System.Net.HttpStatusCode statusCode, string responseBody)> AttemptHttpGetAsync(
        string url, string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = await client.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            return (response.StatusCode, responseBody);
        }
        catch (HttpRequestException ex)
        {
            // Log the exception
            Console.WriteLine($"ERROR PayPalHttpClientService / GET - {url} | {ex.Message}");
            return (System.Net.HttpStatusCode.InternalServerError, null);
        }
    }
}

public class PayPalAccessTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("app_id")]
    public string AppId { get; set; }
}