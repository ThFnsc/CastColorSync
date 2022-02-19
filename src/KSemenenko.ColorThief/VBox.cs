using System;

namespace KSemenenko.ColorThief
{
    internal class VBox
    {
        private readonly int[] histo;

        private int[] avg;

        public int B1;

        public int B2;

        private int? count;

        public int G1;

        public int G2;

        public int R1;

        public int R2;

        private int? volume;

        public VBox(int r1, int r2, int g1, int g2, int b1, int b2, int[] histo)
        {
            R1 = r1;
            R2 = r2;
            G1 = g1;
            G2 = g2;
            B1 = b1;
            B2 = b2;
            this.histo = histo;
        }

        public int Volume(bool force)
        {
            if (!volume.HasValue || force)
            {
                volume = (R2 - R1 + 1) * (G2 - G1 + 1) * (B2 - B1 + 1);
            }

            return volume.Value;
        }

        public int Count(bool force)
        {
            if (!count.HasValue || force)
            {
                int num = 0;
                for (int i = R1; i <= R2; i++)
                {
                    for (int j = G1; j <= G2; j++)
                    {
                        for (int k = B1; k <= B2; k++)
                        {
                            int colorIndex = Mmcq.GetColorIndex(i, j, k);
                            num += histo[colorIndex];
                        }
                    }
                }

                count = num;
            }

            return count.Value;
        }

        public VBox Clone()
        {
            return new VBox(R1, R2, G1, G2, B1, B2, histo);
        }

        public int[] Avg(bool force)
        {
            if (avg == null || force)
            {
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                for (int i = R1; i <= R2; i++)
                {
                    for (int j = G1; j <= G2; j++)
                    {
                        for (int k = B1; k <= B2; k++)
                        {
                            int colorIndex = Mmcq.GetColorIndex(i, j, k);
                            int num5 = histo[colorIndex];
                            num += num5;
                            num2 += Convert.ToInt32(num5 * (i + 0.5) * 8.0);
                            num3 += Convert.ToInt32(num5 * (j + 0.5) * 8.0);
                            num4 += Convert.ToInt32(num5 * (k + 0.5) * 8.0);
                        }
                    }
                }

                if (num > 0)
                {
                    avg = new int[3]
                    {
                        Math.Abs(num2 / num),
                        Math.Abs(num3 / num),
                        Math.Abs(num4 / num)
                    };
                }
                else
                {
                    avg = new int[3]
                    {
                        Math.Abs(8 * (R1 + R2 + 1) / 2),
                        Math.Abs(8 * (G1 + G2 + 1) / 2),
                        Math.Abs(8 * (B1 + B2 + 1) / 2)
                    };
                }
            }

            return avg;
        }

        public bool Contains(int[] pixel)
        {
            int num = pixel[0] >> 3;
            int num2 = pixel[1] >> 3;
            int num3 = pixel[2] >> 3;
            if (num >= R1 && num <= R2 && num2 >= G1 && num2 <= G2 && num3 >= B1)
            {
                return num3 <= B2;
            }

            return false;
        }
    }
}