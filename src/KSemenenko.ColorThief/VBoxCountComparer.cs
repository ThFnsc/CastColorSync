using System.Collections.Generic;

namespace KSemenenko.ColorThief
{
    internal class VBoxCountComparer : IComparer<VBox>
    {
        public int Compare(VBox x, VBox y)
        {
            int num = x.Count(force: false);
            int num2 = y.Count(force: false);
            if (num >= num2)
            {
                if (num <= num2)
                {
                    return 0;
                }

                return 1;
            }

            return -1;
        }
    }
}