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

        public static readonly int TASK_SEQUENCE_COMPLETED = -1;
        public static readonly int TASK_SEQUENCE_IN_PROGRESS = 0;

        public static readonly int TASK_HASH_WANDERING_AROUND = 222;
    }
}
