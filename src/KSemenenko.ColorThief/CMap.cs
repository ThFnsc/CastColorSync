using System;
using System.Collections.Generic;
using System.Linq;

namespace KSemenenko.ColorThief
{
    internal class CMap
    {
        private readonly List<VBox> vboxes = new List<VBox>();

        private List<QuantizedColor> palette;

        public void Push(VBox box)
        {
            palette = null;
            vboxes.Add(box);
        }

        public List<QuantizedColor> GeneratePalette()
        {
            if (palette == null)
            {
                palette = (from vBox in vboxes
                           let rgb = vBox.Avg(force: false)
                           let color = FromRgb(rgb[0], rgb[1], rgb[2])
                           select new QuantizedColor(color, vBox.Count(force: false))).ToList();
            }

            return palette;
        }

        public int Size()
        {
            return vboxes.Count;
        }

        public int[] Map(int[] color)
        {
            using (IEnumerator<VBox> enumerator = vboxes.Where((VBox vbox) => vbox.Contains(color)).GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current.Avg(force: false);
                }
            }

            return Nearest(color);
        }

        public int[] Nearest(int[] color)
        {
            double num = double.MaxValue;
            int[] result = null;
            foreach (VBox vbox in vboxes)
            {
                int[] array = vbox.Avg(force: false);
                double num2 = Math.Sqrt(Math.Pow(color[0] - array[0], 2.0) + Math.Pow(color[1] - array[1], 2.0) + Math.Pow(color[2] - array[2], 2.0));
                if (num2 < num)
                {
                    num = num2;
                    result = array;
                }
            }

            return result;
        }

        public VBox FindColor(double targetLuma, double minLuma, double maxLuma, double targetSaturation, double minSaturation, double maxSaturation)
        {
            VBox vBox = null;
            double num = 0.0;
            int highestPopulation = vboxes.Select((VBox p) => p.Count(force: false)).Max();
            foreach (VBox vbox in vboxes)
            {
                int[] array = vbox.Avg(force: false);
                HslColor hslColor = FromRgb(array[0], array[1], array[2]).ToHsl();
                double s = hslColor.S;
                double l = hslColor.L;
                if (s >= minSaturation && s <= maxSaturation && l >= minLuma && l <= maxLuma)
                {
                    double num2 = Mmcq.CreateComparisonValue(s, targetSaturation, l, targetLuma, vbox.Count(force: false), highestPopulation);
                    if (vBox == null || num2 > num)
                    {
                        vBox = vbox;
                        num = num2;
                    }
                }
            }

            return vBox;
        }

        public Color FromRgb(int red, int green, int blue)
        {
            Color result = default;
            result.A = byte.MaxValue;
            result.R = (byte)red;
            result.G = (byte)green;
            result.B = (byte)blue;
            return result;
        }
    }
}