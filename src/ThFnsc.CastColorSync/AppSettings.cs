using Kevsoft.WLED;

namespace ThFnsc.CastColorSync;

public class AppSettings
{
    public required HassSettings Hass { get; set; }

    public required WLedSettings WLed { get; set; }

    public bool Validate()
    {
        ArgumentNullException.ThrowIfNull(Hass);
        ArgumentNullException.ThrowIfNull(WLed);
        Hass.Validate();
        WLed.Validate();
        return true;
    }
}

public class HassSettings
{
    public string Token { get; set; } = null!;
    
    public string[] SourceDevices { get; set; } = Array.Empty<string>();
        
    public Uri Url { get; set; } = null!;

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(Url);
        if (string.IsNullOrWhiteSpace(Token))
            throw new ArgumentException(nameof(Token));
        if (SourceDevices is null or { Length: 0 })
            throw new ArgumentException(nameof(SourceDevices));
    }
}

public class WLedSettings
{
    public required Uri URI { get; set; }

    public required SegmentRequest SegmentOptions { get; set; }

    public void Validate()
    {
        ArgumentNullException.ThrowIfNull(URI);
    }
}