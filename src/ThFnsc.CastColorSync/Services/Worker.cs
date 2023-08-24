using Microsoft.Extensions.Options;
using Kevsoft.WLED;
using System.Text.Json;

namespace ThFnsc.CastColorSync.Services;

public class Worker : BackgroundService
{
    private string? _lastPicture;
    private readonly IServiceProvider _serviceProvider;
    private readonly IColorPicker _colorPicker;
    private readonly IOptions<AppSettings> _appSettings;
    private StateResponse? _previousLightState;

    public Worker(
        IServiceProvider serviceProvider,
        IColorPicker colorPicker,
        IOptions<AppSettings> appSettings)
    {
        _serviceProvider = serviceProvider;
        _colorPicker = colorPicker;
        _appSettings = appSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider;
                var hassClient = service.GetRequiredService<IHassClient>();
                var appSettings = service.GetRequiredService<IOptions<AppSettings>>().Value;
                var wledClient = service.GetRequiredService<WLedClient>();

                var pictureUrls = await Task.WhenAll(appSettings.Hass.SourceDevices.Select(d => PlayingAlbumImageAsync(hassClient, d)));
                var pictureUrl = pictureUrls.Where(p => p is not null).FirstOrDefault();

                if (pictureUrl != _lastPicture)
                {
                    _lastPicture = pictureUrl;

                    if (pictureUrl is null)
                    {
                        if (_previousLightState is not null)
                            await SetLightStateAsync(wledClient, _previousLightState);
                    }
                    else
                    {
                        _previousLightState = await wledClient.GetState();

                        var palette = await GetPaletteFromImageUrlAsync(hassClient, pictureUrl);

                        await SetLightColorAsync(wledClient, palette);
                    }
                }
                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (TaskCanceledException) { }
            }
        }
        finally
        {
            if (_previousLightState != null)
                await SetLightStateAsync(_serviceProvider.GetRequiredService<WLedClient>(), _previousLightState);
        }
    }

    private static Task SetLightStateAsync(WLedClient wLedClient, StateResponse state)
    {
        var asJson = JsonSerializer.SerializeToUtf8Bytes(state);
        var asRequest = JsonSerializer.Deserialize<StateRequest>(asJson)!;
        return wLedClient.Post(asRequest);
    }

    private static async Task<string?> PlayingAlbumImageAsync(IHassClient hassClient, string entityId)
    {
        var state = await hassClient.States.GetState(entityId);
        if (state.State is "playing" && state.Attributes.TryGetValue("entity_picture_local", out var pictureObject) && pictureObject is string pictureUrl)
            return pictureUrl;
        return null;
    }

    private async Task<Rgb24[]> GetPaletteFromImageUrlAsync(IHassClient hassClient, string pictureUrl)
    {
        var imageStream = await hassClient.GetImageAsync(pictureUrl);
        return await _colorPicker.GetColorPalette(imageStream, 3);
    }

    private Task SetLightColorAsync(WLedClient wLedClient, Rgb24[] color)
    {
        var segment = _appSettings.Value.WLed.SegmentOptions;
        segment.SegmentState = true;
        segment.Colors = color.Select(c => new int[] { c.R, c.G, c.B }).ToArray();

        var newState = new StateRequest()
        {
            On = true,
            Segments = new SegmentRequest[] { segment }
        };

        return wLedClient.Post(newState);
    }
}