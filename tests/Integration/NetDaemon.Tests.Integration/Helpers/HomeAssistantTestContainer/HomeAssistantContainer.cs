using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;

public class HomeAssistantContainer : DockerContainer
{
    private readonly HomeAssistantConfiguration _configuration;
    public ushort Port => GetMappedPublicPort(8123);

    private HttpClient? _client;
    public HttpClient Client => _client ??= new HttpClient
    {
        BaseAddress = new Uri($"http://localhost:{Port}")
    };

    public HomeAssistantContainer(HomeAssistantConfiguration configuration) : base(configuration)
    {
        _configuration = configuration;
    }

    public async Task<HomeAssistantAuthorizeResult> DoOnboarding()
    {
        var onboardingResult = await Client.PostAsync("/api/onboarding/users", JsonContent.Create(new
        {
            client_id = _configuration.ClientId,
            name = "foobar",
            username = _configuration.Username,
            password = _configuration.Password,
            language = "en-GB"
        }));

        onboardingResult.EnsureSuccessStatusCode();

        return (await onboardingResult.Content.ReadFromJsonAsync<HomeAssistantAuthorizeResult>())!;
    }
    public async Task AddIntegrations(string token)
    {
        AddAuthorizationHeaders(token);
        await AddLocalCalendarIntegration();
    }

    private void AddAuthorizationHeaders(string token)
    {
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

    }
    private async Task AddLocalCalendarIntegration()
    {
        var result = await Client.PostAsync("/api/config/config_entries/flow", JsonContent.Create(new
        {
            handler = "local_calendar",
            show_advanced_options = true
        }));
        if (result.IsSuccessStatusCode)
        {
            var stream = await result.Content.ReadAsStreamAsync();
            var json = await JsonSerializer.DeserializeAsync<JsonElement>(stream);
            var flowId = json.GetProperty("flow_id").GetString();
            _ = await Client.PostAsync($"/api/config/config_entries/flow/{flowId}", JsonContent.Create(new
            {
                calendar_name = "cal"
            }));
        }
    }

    public async Task<HomeAssistantTokenResult> GenerateApiToken(string authCode)
    {
        var tokenResponse = await Client.PostAsync("/auth/token", new FormUrlEncodedContent(
            new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", authCode },
                { "client_id", _configuration.ClientId },
            }));

        tokenResponse.EnsureSuccessStatusCode();
        return (await tokenResponse.Content.ReadFromJsonAsync<HomeAssistantTokenResult>())!;
    }
}

public record HomeAssistantAuthorizeResult(
    [property:JsonPropertyName("auth_code")][property:System.Text.Json.Serialization.JsonRequired]string AuthCode);

public record HomeAssistantTokenResult(
    [property:JsonPropertyName("access_token")][property:System.Text.Json.Serialization.JsonRequired]string AccessToken,
    [property:JsonPropertyName("token_type")][property:System.Text.Json.Serialization.JsonRequired]string TokenType,
    [property:JsonPropertyName("refresh_token")][property:System.Text.Json.Serialization.JsonRequired]string RefreshToken,
    [property:JsonPropertyName("expires_in")][property:System.Text.Json.Serialization.JsonRequired]int ExpiresIn,
    [property:JsonPropertyName("ha_auth_provider")][property:System.Text.Json.Serialization.JsonRequired]string HaAuthProvider);
