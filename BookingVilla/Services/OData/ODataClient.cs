using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BookingVilla.Services.OData;

public class ODataClient : IODataClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ODataClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        var response = await client.GetAsync(requestUri, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(responseStream, _serializerOptions, cancellationToken);
        }

        // Nếu server trả lỗi (4xx/5xx), đọc nội dung trả về để dễ debug
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"OData request '{requestUri}' failed with status {(int)response.StatusCode} ({response.ReasonPhrase}). Response: {errorContent}");
    }

    public async Task<bool> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var client = CreateClient();
        var response = await client.DeleteAsync(requestUri, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        // Trả về false cho các mã lỗi phổ biến thay vì ném ngoại lệ,
        // để UI có thể hiển thị thông báo lỗi thân thiện.
        if (response.StatusCode is HttpStatusCode.BadRequest
            or HttpStatusCode.Conflict
            or HttpStatusCode.Forbidden
            or HttpStatusCode.Unauthorized)
        {
            // Optionally read response content for diagnostics (not thrown)
            _ = await response.Content.ReadAsStringAsync(cancellationToken);
            return false;
        }

        response.EnsureSuccessStatusCode();
        return false;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("ODataClient");
        var context = _httpContextAccessor.HttpContext
                      ?? throw new InvalidOperationException("HTTP context is not available.");

        var baseUri = new Uri($"{context.Request.Scheme}://{context.Request.Host}");
        client.BaseAddress = baseUri;
        // Nếu request vào đã có header Authorization, ưu tiên dùng header đó (ví dụ khi user đăng nhập qua API/Swagger)
        var incomingAuth = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(incomingAuth))
        {
            if (AuthenticationHeaderValue.TryParse(incomingAuth, out var parsedAuth))
            {
                client.DefaultRequestHeaders.Authorization = parsedAuth;
            }
        }
        else
        {
            // Nếu không có header Authorization, kiểm tra session
            if (context.Session != null && context.Session.IsAvailable)
            {
                var token = context.Session.GetString("AccessToken");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    client.DefaultRequestHeaders.Authorization = null;
                }
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = null;
            }
        }

        return client;
    }
}

