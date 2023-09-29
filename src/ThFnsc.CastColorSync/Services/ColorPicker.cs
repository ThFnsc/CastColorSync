using ColorHelper;

namespace ThFnsc.CastColorSync.Services;

public class ColorPicker : IColorPicker
{
    private const int _resizeTo = 32;

    public async Task<Rgb24[]> GetColorPalette(Stream imageStream, int colors)
    {
        using var image = await Image.LoadAsync<Rgb24>(imageStream);
        image.Mutate(x => x.Resize(_resizeTo, _resizeTo));

        var (pixels, weights) = ImageToArray(image);
        var kmeans = new Accord.MachineLearning.KMeans(colors);

        var clusters = kmeans.Learn(pixels, weights);
        var distincts = kmeans.Centroids;

        var palette = new Rgb24[colors];
        for (var i = 0; i < colors; i++)
            palette[i] = new((byte)distincts[i][0], (byte)distincts[i][1], (byte)distincts[i][2]);

        return palette;
    }

    private static (double[][] pixels, double[] weights) ImageToArray(Image<Rgb24> image)
    {
        int width = image.Width;
        int height = image.Height;

        double[][] pixels = new double[width * height][];
        double[] weights = new double[width * height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var pixel = image[x, y];
                var hsl = ColorConverter.RgbToHsl(new(pixel.R, pixel.G, pixel.B));
                var position = y * width + x;
                pixels[position] = new double[] { pixel.R, pixel.G, pixel.B };
                weights[position] = (hsl.S / 255.0) * (hsl.L / 255.0);
            }

        if (weights.All(w => w == 0))
            Array.Fill(weights, 1);
        return (pixels, weights);
    }
}