namespace ThFnsc.CastColorSync;

public class AppSettings
{
    public HassSettings? Hass { get; set; }
}

public class HassSettings
{
    public string? Token { get; set; }
    public string? SourceDevice { get; set; }
    public string[]? Lights { get; set; }
    public Uri? Url { get; set; }
}