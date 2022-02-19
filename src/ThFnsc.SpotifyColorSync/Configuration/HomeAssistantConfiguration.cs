using HADotNet.Core;
using HADotNet.Core.Clients;
using Microsoft.Extensions.Options;
using ThFnsc.SpotifyColorSync.Services;

namespace ThFnsc.SpotifyColorSync.Configuration;

public static class HomeAssistantConfiguration
{
    public static IServiceCollection AddHass(this IServiceCollection services)
    {
        services.AddHttpClient<IHassClient, HassClient>((httpClient, provider) =>
        {
            var appSettings = provider.GetRequiredService<IOptions<AppSettings>>();
            if (appSettings.Value.Hass is null)
                throw new NullReferenceException(nameof(appSettings.Value.Hass));

            if (!ClientFactory.IsInitialized)
                ClientFactory.Initialize(appSettings.Value.Hass.Url, appSettings.Value.Hass.Token);
            httpClient.BaseAddress = appSettings.Value.Hass.Url;
            httpClient.DefaultRequestHeaders.Authorization = new("Bearer", appSettings.Value.Hass.Token);

            return new HassClient(
                httpClient: httpClient,
                entity: ClientFactory.GetClient<EntityClient>(),
                states: ClientFactory.GetClient<StatesClient>(),
                service: ClientFactory.GetClient<ServiceClient>(),
                discovery: ClientFactory.GetClient<DiscoveryClient>());
        });
        return services;
    }
}
