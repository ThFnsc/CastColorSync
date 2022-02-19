using System;

namespace KSemenenko.ColorThief
{
    public struct Color
    {
        public byte A;

        public byte B;

        public byte G;

        public byte R;

        public HslColor ToHsl()
        {
            double num = 0.00392156862745098 * R;
            double num2 = 0.00392156862745098 * G;
            double num3 = 0.00392156862745098 * B;
            double num4 = Math.Max(Math.Max(num, num2), num3);
            double num5 = Math.Min(Math.Min(num, num2), num3);
            double num6 = num4 - num5;
            double num7 = num6 == 0.0 ? 0.0 : num4 == num ? (num2 - num3) / num6 % 6.0 : num4 != num2 ? 4.0 + (num - num2) / num6 : 2.0 + (num3 - num) / num6;
            double num8 = 0.5 * (num4 - num5);
            double s = num6 == 0.0 ? 0.0 : num6 / (1.0 - Math.Abs(2.0 * num8 - 1.0));
            HslColor result = default;
            result.H = 60.0 * num7;
            result.S = s;
            result.L = num8;
            result.A = 0.00392156862745098 * A;
            return result;
        }

        public string ToHexString()
        {
            return "#" + R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
        }

        public string ToHexAlphaString()
        {
            return "#" + A.ToString("X2") + R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
        }

        public override string ToString()
        {
            if (A == byte.MaxValue)
            {
                return ToHexString();
            }

            return ToHexAlphaString();
        }
    }
}