using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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

    public HomeAssistantContainer(HomeAssistantConfiguration configuration, ILogger logger) : base(configuration, logger)
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
    [property:JsonPropertyName("auth_code")][property:JsonRequired]string AuthCode);
    
public record HomeAssistantTokenResult(
    [property:JsonPropertyName("access_token")][property:JsonRequired]string AccessToken, 
    [property:JsonPropertyName("token_type")][property:JsonRequired]string TokenType, 
    [property:JsonPropertyName("refresh_token")][property:JsonRequired]string RefreshToken, 
    [property:JsonPropertyName("expires_in")][property:JsonRequired]int ExpiresIn, 
    [property:JsonPropertyName("ha_auth_provider")][property:JsonRequired]string HaAuthProvider);