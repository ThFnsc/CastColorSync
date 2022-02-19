using ColorHelper;

namespace ThFnsc.CastColorSync.Services;

public interface IColorPicker
{
    Task<HSL> GetSignatureColorAsync(Stream imageStream);
}