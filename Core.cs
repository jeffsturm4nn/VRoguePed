using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRoguePed
{
    internal class Core
    {
        public static bool IsModActive = false;
        public static List<Ped> RoguePeds = new List<Ped> ();
        public static List<ControlKey> ControlKeys = new List<ControlKey> ();

        public static void ToggleModActiveProc()
        {
            IsModActive = !IsModActive;
        }
    }
}
