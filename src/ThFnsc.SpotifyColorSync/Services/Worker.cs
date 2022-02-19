using KSemenenko.ColorThief;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ThFnsc.SpotifyColorSync.Services;

public class Worker : BackgroundService
{
    private string? _lastPicture;
    private readonly ColorThief _colorThief;
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<string> _lightsTurnedOn = new();

    public Worker(IServiceProvider serviceProvider)
    {
        _colorThief = new ColorThief();
        _serviceProvider = serviceProvider;
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

    private async Task<QuantizedColor> GetColorFromImageUrl(IHassClient hassClient, string pictureUrl)
    {
        var imageStream = await hassClient.GetImageAsync(pictureUrl);
        var image = Image.Load<Rgba32>(imageStream);
        return _colorThief.GetPalette(image).Last();
    }

    private static Task SetLightsColorAsync(IHassClient hassClient, string[] entityIds, QuantizedColor color)
    {
        var hsl = ColorHelper.ColorConverter.RgbToHsv(new(color.Color.R, color.Color.G, color.Color.B));

        var tasks = entityIds.Select(e => hassClient.Service.CallService("light.turn_on", new
        {
            entity_id = e,
            hs_color = new int[] { hsl.H, hsl.S < 5 ? 0 : 100 }
        }));

        return Task.WhenAll(tasks);
    }
}
