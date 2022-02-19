using System;
using System.Collections.Generic;

namespace KSemenenko.ColorThief
{
    internal static class Mmcq
    {
        public const int Sigbits = 5;

        public const int Rshift = 3;

        public const int Mult = 8;

        public const int Histosize = 32768;

        public const int VboxLength = 32;

        public const double FractByPopulation = 0.75;

        public const int MaxIterations = 1000;

        public const double WeightSaturation = 3.0;

        public const double WeightLuma = 6.0;

        public const double WeightPopulation = 1.0;

        private static readonly VBoxComparer ComparatorProduct = new VBoxComparer();

        private static readonly VBoxCountComparer ComparatorCount = new VBoxCountComparer();

        public static int GetColorIndex(int r, int g, int b)
        {
            return (r << 10) + (g << 5) + b;
        }

        private static int[] GetHisto(IEnumerable<byte[]> pixels)
        {
            int[] array = new int[32768];
            foreach (byte[] pixel in pixels)
            {
                int r = pixel[0] >> 3;
                int g = pixel[1] >> 3;
                int b = pixel[2] >> 3;
                int colorIndex = GetColorIndex(r, g, b);
                array[colorIndex]++;
            }

            return array;
        }

        private static VBox VboxFromPixels(IList<byte[]> pixels, int[] histo)
        {
            int num = 1000000;
            int num2 = 0;
            int num3 = 1000000;
            int num4 = 0;
            int num5 = 1000000;
            int num6 = 0;
            int count = pixels.Count;
            for (int i = 0; i < count; i++)
            {
                byte[] array = pixels[i];
                int num7 = array[0] >> 3;
                int num8 = array[1] >> 3;
                int num9 = array[2] >> 3;
                if (num7 < num)
                {
                    num = num7;
                }
                else if (num7 > num2)
                {
                    num2 = num7;
                }

                if (num8 < num3)
                {
                    num3 = num8;
                }
                else if (num8 > num4)
                {
                    num4 = num8;
                }

                if (num9 < num5)
                {
                    num5 = num9;
                }
                else if (num9 > num6)
                {
                    num6 = num9;
                }
            }

            return new VBox(num, num2, num3, num4, num5, num6, histo);
        }

        private static VBox[] DoCut(char color, VBox vbox, IList<int> partialsum, IList<int> lookaheadsum, int total)
        {
            int num;
            int num2;
            switch (color)
            {
                case 'r':
                    num = vbox.R1;
                    num2 = vbox.R2;
                    break;
                case 'g':
                    num = vbox.G1;
                    num2 = vbox.G2;
                    break;
                default:
                    num = vbox.B1;
                    num2 = vbox.B2;
                    break;
            }

            for (int i = num; i <= num2; i++)
            {
                if (partialsum[i] > total / 2)
                {
                    VBox vBox = vbox.Clone();
                    VBox vBox2 = vbox.Clone();
                    int num3 = i - num;
                    int num4 = num2 - i;
                    int j;
                    for (j = num3 <= num4 ? Math.Min(num2 - 1, Math.Abs(i + num4 / 2)) : Math.Max(num, Math.Abs(Convert.ToInt32(i - 1 - num3 / 2.0))); j < 0 || partialsum[j] <= 0; j++)
                    {
                    }

                    int num5 = lookaheadsum[j];
                    while (num5 == 0 && j > 0 && partialsum[j - 1] > 0)
                    {
                        num5 = lookaheadsum[--j];
                    }

                    switch (color)
                    {
                        case 'r':
                            vBox.R2 = j;
                            vBox2.R1 = j + 1;
                            break;
                        case 'g':
                            vBox.G2 = j;
                            vBox2.G1 = j + 1;
                            break;
                        default:
                            vBox.B2 = j;
                            vBox2.B1 = j + 1;
                            break;
                    }

                    return new VBox[2]
                    {
                        vBox,
                        vBox2
                    };
                }
            }

            throw new Exception("VBox can't be cut");
        }

        private static VBox[] MedianCutApply(IList<int> histo, VBox vbox)
        {
            if (vbox.Count(force: false) == 0)
            {
                return null;
            }

            if (vbox.Count(force: false) == 1)
            {
                return new VBox[2]
                {
                    vbox.Clone(),
                    null
                };
            }

            int num = vbox.R2 - vbox.R1 + 1;
            int num2 = vbox.G2 - vbox.G1 + 1;
            int val = vbox.B2 - vbox.B1 + 1;
            int num3 = Math.Max(Math.Max(num, num2), val);
            int num4 = 0;
            int[] array = new int[32];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = -1;
            }

            int[] array2 = new int[32];
            for (int j = 0; j < array2.Length; j++)
            {
                array2[j] = -1;
            }

            if (num3 == num)
            {
                for (int k = vbox.R1; k <= vbox.R2; k++)
                {
                    int num5 = 0;
                    for (int l = vbox.G1; l <= vbox.G2; l++)
                    {
                        for (int m = vbox.B1; m <= vbox.B2; m++)
                        {
                            int colorIndex = GetColorIndex(k, l, m);
                            num5 += histo[colorIndex];
                        }
                    }

                    num4 = array[k] = num4 + num5;
                }
            }
            else if (num3 == num2)
            {
                for (int k = vbox.G1; k <= vbox.G2; k++)
                {
                    int num5 = 0;
                    for (int l = vbox.R1; l <= vbox.R2; l++)
                    {
                        for (int m = vbox.B1; m <= vbox.B2; m++)
                        {
                            int colorIndex = GetColorIndex(l, k, m);
                            num5 += histo[colorIndex];
                        }
                    }

                    num4 = array[k] = num4 + num5;
                }
            }
            else
            {
                for (int k = vbox.B1; k <= vbox.B2; k++)
                {
                    int num5 = 0;
                    for (int l = vbox.R1; l <= vbox.R2; l++)
                    {
                        for (int m = vbox.G1; m <= vbox.G2; m++)
                        {
                            int colorIndex = GetColorIndex(l, m, k);
                            num5 += histo[colorIndex];
                        }
                    }

                    num4 = array[k] = num4 + num5;
                }
            }

            for (int k = 0; k < 32; k++)
            {
                if (array[k] != -1)
                {
                    array2[k] = num4 - array[k];
                }
            }

            if (num3 != num)
            {
                if (num3 != num2)
                {
                    return DoCut('b', vbox, array, array2, num4);
                }

                return DoCut('g', vbox, array, array2, num4);
            }

            return DoCut('r', vbox, array, array2, num4);
        }

        private static void Iter(List<VBox> lh, IComparer<VBox> comparator, int target, IList<int> histo)
        {
            int num = 1;
            int num2 = 0;
            while (num2 < 1000)
            {
                VBox vBox = lh[lh.Count - 1];
                if (vBox.Count(force: false) == 0)
                {
                    lh.Sort(comparator);
                    num2++;
                    continue;
                }

                lh.RemoveAt(lh.Count - 1);
                VBox[] array = MedianCutApply(histo, vBox);
                VBox vBox2 = array[0];
                VBox vBox3 = array[1];
                if (vBox2 == null)
                {
                    throw new Exception("vbox1 not defined; shouldn't happen!");
                }

                lh.Add(vBox2);
                if (vBox3 != null)
                {
                    lh.Add(vBox3);
                    num++;
                }

                lh.Sort(comparator);
                if (num < target && num2++ <= 1000)
                {
                    continue;
                }

                break;
            }
        }

        public static CMap Quantize(byte[][] pixels, int maxcolors)
        {
            if (pixels.Length == 0 || maxcolors < 2 || maxcolors > 256)
            {
                return null;
            }

            int[] histo = GetHisto(pixels);
            VBox item = VboxFromPixels(pixels, histo);
            List<VBox> list = new List<VBox>
            {
                item
            };
            int target = (int)Math.Ceiling(0.75 * maxcolors);
            Iter(list, ComparatorCount, target, histo);
            list.Sort(ComparatorProduct);
            Iter(list, ComparatorProduct, maxcolors - list.Count, histo);
            list.Reverse();
            CMap cMap = new CMap();
            foreach (VBox item2 in list)
            {
                cMap.Push(item2);
            }

            return cMap;
        }

        public static double CreateComparisonValue(double saturation, double targetSaturation, double luma, double targetLuma, int population, int highestPopulation)
        {
            double[] obj = new double[6]
            {
                0.0,
                3.0,
                0.0,
                6.0,
                0.0,
                1.0
            };
            obj[0] = InvertDiff(saturation, targetSaturation);
            obj[2] = InvertDiff(luma, targetLuma);
            obj[4] = population / (double)highestPopulation;
            return WeightedMean(obj);
        }

        private static double WeightedMean(params double[] values)
        {
            double num = 0.0;
            double num2 = 0.0;
            for (int i = 0; i < values.Length; i += 2)
            {
                double num3 = values[i];
                double num4 = values[i + 1];
                num += num3 * num4;
                num2 += num4;
            }

            return num / num2;
        }

        private static double InvertDiff(double value, double targetValue)
        {
            return 1.0 - Math.Abs(value - targetValue);
        }
    }
}