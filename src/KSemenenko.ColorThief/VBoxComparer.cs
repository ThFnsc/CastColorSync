using System.Collections.Generic;

namespace KSemenenko.ColorThief
{
    internal class VBoxComparer : IComparer<VBox>
    {
        public int Compare(VBox x, VBox y)
        {
            int num = x.Count(force: false);
            int num2 = y.Count(force: false);
            int num3 = x.Volume(force: false);
            int num4 = y.Volume(force: false);
            int num5 = num * num3;
            int num6 = num2 * num4;
            if (num5 >= num6)
            {
                if (num5 <= num6)
                {
                    return 0;
                }

                return 1;
            }

            return -1;
        }
    }
}