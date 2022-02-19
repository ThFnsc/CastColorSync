using ColorHelper;

namespace ThFnsc.SpotifyColorSync.Services;

public interface IColorPicker
{
    Task<HSL> GetSignatureColorAsync(Stream imageStream);
}