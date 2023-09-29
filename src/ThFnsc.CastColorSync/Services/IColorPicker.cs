using ColorHelper;

namespace ThFnsc.CastColorSync.Services;

public interface IColorPicker
{
    Task<Rgb24[]> GetColorPalette(Stream imageStream, int colors);
}