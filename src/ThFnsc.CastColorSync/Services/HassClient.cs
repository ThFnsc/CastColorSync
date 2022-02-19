using HADotNet.Core.Clients;

namespace ThFnsc.CastColorSync.Services;

public class HassClient : IHassClient
{
    private readonly HttpClient _httpClient;
    public EntityClient Entity { get; }
    public StatesClient States { get; }
    public ServiceClient Service { get; }
    public DiscoveryClient Discovery { get; }

    public HassClient(HttpClient httpClient, EntityClient entity, StatesClient states, ServiceClient service, DiscoveryClient discovery)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        States = states ?? throw new ArgumentNullException(nameof(states));
        Service = service ?? throw new ArgumentNullException(nameof(service));
        Discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
    }

    public async Task<Stream> GetImageAsync(string url)
    {
        var res = await _httpClient.GetAsync(url);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStreamAsync();
    }
}
