using ColorHelper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ThFnsc.CastColorSync.Services;
public class ColorPicker : IColorPicker
{
    private const int _resizeTo = 32;
    private const float _scoreCutoff = .1f;

    public async Task<HSL> GetSignatureColorAsync(Stream imageStream)
    {
        using var image = await Image.LoadAsync<Rgb24>(imageStream);
        image.Mutate(x => x.Resize(_resizeTo, _resizeTo));

        var hslPixels = GetHSLPixels(image);

        var histogram = MakeHistogram(hslPixels);

        var recordIndex = GetRecordIndex(histogram);

        if (recordIndex == -1)
            return new HSL(0, 0, 100);
        return new HSL(recordIndex, 100, 50);
    }

    private static int GetRecordIndex(float[] input)
    {
        const int around = 3;
        const int total = around * 2 + 1;
        var mask = new float[total];
        for (int i = 0; i <= around; i++)
        {
            var relevance = 1.0f / (i + 1);
            mask[around + i] = relevance;
            mask[around - i] = relevance;
        }

        int recordIndex = -1;
        float recordAverage = 0;
        for (int i = 0; i < input.Length; i++)
        {
            float average = 0;
            var offset = i - around;

            for (int j = 0; j < total; j++)
            {
                var index = offset + j;
                if (index < 0)
                    index = input.Length + index;
                else if (index >= input.Length)
                    index -= input.Length;
                average += mask[j] * input[index];
            }

            if (average > recordAverage)
            {
                recordAverage = average;
                recordIndex = i;
            }
        }
        return recordIndex;
    }

    private static float[] MakeHistogram(HSL[] hslPixels)
    {
        var histogram = new float[361];
        for (int i = 0; i < hslPixels.Length; i++)
        {
            var pixel = hslPixels[i];
            var lightnessScore = (50 - Math.Abs(pixel.L - 50)) / 50.0f;
            var saturationScore = (float)Math.Pow(pixel.S / 100f, 2);
            var finalScore = saturationScore * lightnessScore;
            if (finalScore > _scoreCutoff)
                histogram[pixel.H] += (float)Math.Pow(finalScore, 4);
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