using GTA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static VRoguePed.Constants;

namespace VRoguePed
{
    internal class Core
    {
        public static bool ModActive = false;
        public static bool KeepRoguePeds = false;
        public static List<RoguePed> RoguePeds = new List<RoguePed>();
        public static List<Ped> VictimPeds = new List<Ped>();
        public static List<Ped> ProcessedPeds = new List<Ped>();
        public static List<ControlKey> ControlKeys = new List<ControlKey>();

        public static void ToggleModActiveProc()
        {
            ModActive = !ModActive;

            if (ModActive)
            {
                Util.Subtitle("{ VRoguePed Enabled }", 1700);
            }
            else
            {
                Util.Subtitle("{ VRoguePed Disabled }", 1700);
            }
        }

        public static void ReloadConfigFileProc()
        {
            ProcessConfigFile(true);
            Util.Notify("VRoguePed: Config file reloaded.");
        }

        public static void ProcessConfigFile(bool isReloadingConfigs)
        {
            try
            {
                ScriptSettings Settings = ScriptSettings.Load(CONFIG_FILE_PATH);

                if (!File.Exists(CONFIG_FILE_PATH))
                {
                    Util.Notify("VRoguePed Config File Error:\n" + CONFIG_FILE_PATH + " could not be found.\nAll settings were set to default.", true);
                }

                if (!isReloadingConfigs)
                {
                    ModActive = Settings.GetValue("GLOBAL_VARS", "ENABLE_ON_GAME_LOAD", false);

                    InputModule.InitControlKeysFromConfig(Settings); 
                    InputModule.SortKeyTuples();
                    
                }


                PedModule.ReadPedParamsFromConfig(Settings);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ProcessConfigFile() Error: " + e.ToString(), false);
            }

        }
    }
}
