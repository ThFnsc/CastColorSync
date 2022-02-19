using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KSemenenko.ColorThief
{

    public class ColorThief
    {
        public const int DefaultColorCount = 5;

        public const int DefaultQuality = 10;

        public const bool DefaultIgnoreWhite = true;

        public const int ColorDepth = 4;

        public QuantizedColor GetColor(Image<Rgba32> sourceImage, int quality = 10, bool ignoreWhite = true)
        {
            List<QuantizedColor> palette = GetPalette(sourceImage, 3, quality, ignoreWhite);
            Color color = default;
            color.A = Convert.ToByte(palette.Average((QuantizedColor a) => a.Color.A));
            color.R = Convert.ToByte(palette.Average((QuantizedColor a) => a.Color.R));
            color.G = Convert.ToByte(palette.Average((QuantizedColor a) => a.Color.G));
            color.B = Convert.ToByte(palette.Average((QuantizedColor a) => a.Color.B));
            return new QuantizedColor(color, Convert.ToInt32(palette.Average((QuantizedColor a) => a.Population)));
        }

        public List<QuantizedColor> GetPalette(Image<Rgba32> sourceImage, int colorCount = 5, int quality = 10, bool ignoreWhite = true)
        {
            byte[][] pixelsFast = GetPixelsFast(sourceImage, quality, ignoreWhite);
            CMap colorMap = GetColorMap(pixelsFast, colorCount);
            if (colorMap != null)
            {
                return colorMap.GeneratePalette();
            }

            return new List<QuantizedColor>();
        }

        private byte[][] GetPixelsFast(Image<Rgba32> sourceImage, int quality, bool ignoreWhite)
        {
            if (quality < 1)
            {
                quality = 10;
            }

            byte[] intFromPixel = GetIntFromPixel(sourceImage);
            int pixelCount = sourceImage.Width * sourceImage.Height;
            return ConvertPixels(intFromPixel, pixelCount, quality, ignoreWhite);
        }

        private byte[] GetIntFromPixel(Image<Rgba32> bmp)
        {
            byte[] array = new byte[bmp.Width * bmp.Height * 4];
            int num = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    var pixel = bmp[i, j];
                    array[num] = pixel.B;
                    num++;
                    array[num] = pixel.G;
                    num++;
                    array[num] = pixel.R;
                    num++;
                    array[num] = pixel.A;
                    num++;
                }
            }

            return array;
        }

        private CMap GetColorMap(byte[][] pixelArray, int colorCount)
        {
            if (colorCount > 0)
            {
                colorCount--;
            }

            return Mmcq.Quantize(pixelArray, colorCount);
        }

        private byte[][] ConvertPixels(byte[] pixels, int pixelCount, int quality, bool ignoreWhite)
        {
            int num = pixelCount * 4;
            if (num != pixels.Length)
            {
                throw new ArgumentException("(expectedDataLength = " + num + ") != (pixels.length = " + pixels.Length + ")");
            }

            int num2 = (pixelCount + quality - 1) / quality;
            int num3 = 0;
            byte[][] array = new byte[num2][];
            for (int i = 0; i < pixelCount; i += quality)
            {
                int num4 = i * 4;
                byte b = pixels[num4];
                byte b2 = pixels[num4 + 1];
                byte b3 = pixels[num4 + 2];
                if (pixels[num4 + 3] >= 125 && (!ignoreWhite || b3 <= 250 || b2 <= 250 || b <= 250))
                {
                    array[num3] = new byte[3]
                    {
                        b3,
                        b2,
                        b
                    };
                    num3++;
                }
            }

            byte[][] array2 = new byte[num3][];
            Array.Copy(array, array2, num3);
            return array2;
        }
    }
}