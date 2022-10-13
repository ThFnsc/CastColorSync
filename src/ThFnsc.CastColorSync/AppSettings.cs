namespace ThFnsc.CastColorSync;

public class AppSettings
{
    public HassSettings Hass { get; set; } = new();

    public bool Validate()
    {
        Hass.Validate();
        return true;
    }
}

public class HassSettings
{
    public string Token { get; set; } = null!;
    public string[] SourceDevices { get; set; } = Array.Empty<string>();
    public string[] Lights { get; set; } = Array.Empty<string>();
    public Uri Url { get; set; } = null!;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Token))
            throw new ArgumentException(nameof(Token));
        if (Url is null)
            throw new ArgumentNullException(nameof(Url));
        if (SourceDevices is null or { Length: 0 })
            throw new ArgumentException(nameof(SourceDevices));
        if (Lights is null or { Length: 0 })
            throw new ArgumentException(nameof(Lights));
    }
}