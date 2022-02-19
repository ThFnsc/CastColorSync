using Microsoft.Extensions.Options;
using ColorHelper;

namespace ThFnsc.CastColorSync.Services;

public class Worker : BackgroundService
{
    private string? _lastPicture;
    private readonly IServiceProvider _serviceProvider;
    private readonly IColorPicker _colorPicker;
    private readonly HashSet<string> _lightsTurnedOn = new();

    public Worker(
        IServiceProvider serviceProvider,
        IColorPicker colorPicker)
    {
        _serviceProvider = serviceProvider;
        _colorPicker = colorPicker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider;
            var hassClient = service.GetRequiredService<IHassClient>();
            var appSettings = service.GetRequiredService<IOptions<AppSettings>>().Value;

            if (appSettings.Hass is null)
                throw new ArgumentException(nameof(appSettings.Hass));
            if (appSettings.Hass.Lights is null)
                throw new ArgumentException(nameof(appSettings.Hass.Lights));

            var pictureUrl = await PlayingAlbumImageAsync(hassClient, appSettings);

            if (pictureUrl != _lastPicture)
            {
                _lastPicture = pictureUrl;

                if (pictureUrl is null)
                    await TurnLightsBackOffAsync(hassClient);
                else
                {
                    var states = await Task.WhenAll(appSettings.Hass.Lights.Select(async light => (light, state: await GetLightStateAsync(hassClient, light))));

                    foreach (var (light, state) in states.Where(t => !t.state))
                        _lightsTurnedOn.Add(light);

                    var color = await GetColorFromImageUrl(hassClient, pictureUrl);

                    await SetLightsColorAsync(hassClient, appSettings.Hass.Lights, color);
                }
            }
            try
            {
                await Task.Delay(1000, stoppingToken);
            }
            catch (TaskCanceledException) { }
        }
        await TurnLightsBackOffAsync(_serviceProvider.GetRequiredService<IHassClient>());
    }

    private static Task TurnLightOffAsync(IHassClient hassClient, string entityId) =>
        hassClient.Service.CallService("light.turn_off", new
        {
            entity_id = entityId
        });

    private async Task TurnLightsBackOffAsync(IHassClient hassClient)
    {
        await Task.WhenAll(_lightsTurnedOn.Select(l => TurnLightOffAsync(hassClient, l)));
        _lightsTurnedOn.Clear();
    }

    private static async Task<string?> PlayingAlbumImageAsync(IHassClient hassClient, AppSettings appSettings)
    {
        var state = await hassClient.States.GetState(appSettings.Hass?.SourceDevice);
        if (state.State is "playing" && state.Attributes.TryGetValue("entity_picture_local", out var pictureObject) && pictureObject is string pictureUrl)
            return pictureUrl;
        return null;
    }

    private static async Task<bool> GetLightStateAsync(IHassClient hassClient, string entityId)
    {
        var state = await hassClient.States.GetState(entityId);
        return state.State is "on";
    }

    private async Task<HSL> GetColorFromImageUrl(IHassClient hassClient, string pictureUrl)
    {
        var imageStream = await hassClient.GetImageAsync(pictureUrl);
        return await _colorPicker.GetSignatureColorAsync(imageStream);
    }

    private static Task SetLightsColorAsync(IHassClient hassClient, string[] entityIds, HSL color)
    {
        var tasks = entityIds.Select(e => hassClient.Service.CallService("light.turn_on", new
        {
            entity_id = e,
            hs_color = new int[] { color.H, color.S < 5 ? 0 : 100 }
        }));

        return Task.WhenAll(tasks);
    }
}