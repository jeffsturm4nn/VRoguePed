using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRoguePed
{
    internal class Constants
    {
        public const int UPDATE_INTERVAL = 13;
        public static readonly string CONFIG_FILE_PATH = (Directory.GetCurrentDirectory() + "\\scripts\\VRoguePed.ini");
    }
}
