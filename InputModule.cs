using System;
using System.Collections.Generic;
using System.Windows.Forms;

using GTA;

using static VRoguePed.Core;
using static VRoguePed.PedModule;

/*
 * 
 * created by jeffsturm4nn
 * 
 */

namespace VRoguePed
{
    public static class InputModule
    {

        public static void InitControlKeysFromConfig(ScriptSettings settings)
        {
            RegisterControlKey("ToggleModActiveKey", settings.GetValue<String>("CONTROL_KEYBOARD", "ToggleModActiveKey", "None"),
                (Action)ToggleModActiveProc, TriggerCondition.PRESSED);

            RegisterControlKey("ReloadConfigFileKey", settings.GetValue<String>("CONTROL_KEYBOARD", "ReloadConfigFileKey", "None"),
                (Action)ReloadConfigFileProc, TriggerCondition.PRESSED);
            
            RegisterControlKey("MakePedGoRogueKey", settings.GetValue<String>("CONTROL_KEYBOARD", "MakePedGoRogueKey", "None"),
                (Action)MakePedGoRogueProc, TriggerCondition.PRESSED);

            //RegisterControlKey("ToggleDebugInfoKey", Settings.GetValue<String>("DEV_STUFF", "ToggleDebugInfoKey", "None"),
            //(Action)delegate { DebugMode = !DebugMode; }, TriggerCondition.PRESSED, true);
        }


        public static void RegisterControlKey(String name, String keyData, Action callback, TriggerCondition condition, bool ignoreInvalid = false)
        {
            List<Keys> keys = Keyboard.TranslateKeyDataToKeyList(keyData);

            if (keys.Count == 0)
            {
                if (!ignoreInvalid)
                    Util.Notify("VRoguePed ControlKey Error:\n Key combination for \"" + name + "\" is invalid. \nThe control was disabled.");

                return;
            }

            ControlKeys.Add(new ControlKey(name, keys, callback, condition));
        }

        public static void CheckForKeysHeldDown()
        {
            for (int i = 0; i < ControlKeys.Count; i++)
            {
                var controlKey = ControlKeys[i];

                if (controlKey.condition == TriggerCondition.HELD && Keyboard.IsKeyListPressed(controlKey.keys))
                {
                    controlKey.callback.Invoke();
                    controlKey.wasPressed = true;
                    break;
                }
            }
        }

        public static void CheckForKeysReleased()
        {
            for (int i = 0; i < ControlKeys.Count; i++)
            {
                var control = ControlKeys[i];

                if (control.wasPressed)
                {
                    if (Keyboard.IsKeyListUp(control.keys))
                    {
                        if (control.condition.HasFlag(TriggerCondition.RELEASED))
                        {
                            control.callback.Invoke();
                        }

                        control.wasPressed = false;
                        //break;
                    }
                }
            }
        }
    }
}
