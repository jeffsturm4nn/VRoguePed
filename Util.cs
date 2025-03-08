using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRoguePed
{
    internal class Util
    {
        public static void Notify(string message, bool important = false)
        {
            GTA.UI.Notify(message, important);
        }

        public static void Subtitle(string message, int durationInMs = 1900)
        {
            GTA.UI.ShowSubtitle(message, durationInMs);
        }
    }
}
