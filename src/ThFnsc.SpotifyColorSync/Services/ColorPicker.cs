using ColorHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ThFnsc.SpotifyColorSync.Services;
public class ColorPicker : IColorPicker
{
    private const int _resizeTo = 32;
    private const byte _saturationCutoff = 30;

    public async Task<HSL> GetSignatureColorAsync(Stream imageStream)
    {
        using var image = await Image.LoadAsync<Rgb24>(imageStream);
        image.Mutate(x => x.Resize(_resizeTo, _resizeTo));

        var hslPixels = GetHSLPixels(image);

        var histogram = MakeHistogram(hslPixels);

        var recordIndex = GetRecordIndex(histogram);

        return new HSL(recordIndex, (byte)(histogram[recordIndex] == 0 ? 0 : 100), 50);
    }

    private static int GetRecordIndex(IList<float> input)
    {
        int recordIndex = 0;
        for (int i = 0; i < input.Count; i++)
            if (input[i] > input[recordIndex])
                recordIndex = i;
        return recordIndex;
    }

    private static float[] MakeHistogram(HSL[] hslPixels)
    {
        var histogram = new float[361];
        for (int i = 0; i < hslPixels.Length; i++)
        {
            var pixel = hslPixels[i];
            if (pixel.S < _saturationCutoff) continue;
            histogram[pixel.H] += pixel.S;
        }
        return histogram;
    }

    private static HSL[] GetHSLPixels(Image<Rgb24> image)
    {
        var rgbPixels = new Rgb24[image.Width * image.Height];
        var hslPixels = new HSL[rgbPixels.Length];

        image.CopyPixelDataTo(rgbPixels);
        for (int i = 0; i < rgbPixels.Length; i++)
        {
            var pixel = rgbPixels[i];
            hslPixels[i] = ColorConverter.RgbToHsl(new(pixel.R, pixel.G, pixel.B));
        }

        return hslPixels;
    }
}