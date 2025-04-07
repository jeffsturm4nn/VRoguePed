using System;
using System.Collections.Generic;
using System.Windows.Forms;

using GTA;

using static VRoguePed.Core;
using static VRoguePed.PedModule;
using static VRoguePed.TestModule;

/*
 * 
 * created by jeffsturm4nn
 * 
 */

namespace VRoguePed
{
    public static class InputModule
    {
        public static void SortKeyTuples()
        {
            for (int i = 0; i < ControlKeys.Count; i++)
            {
                for (int j = 0; j < ControlKeys.Count - 1; j++)
                {
                    if (ControlKeys[j].keys.Count < ControlKeys[j + 1].keys.Count)
                    {
                        var keyPair = ControlKeys[j];

                        ControlKeys[j] = ControlKeys[j + 1];
                        ControlKeys[j + 1] = keyPair;
                    }
                }
            }
        }

        public static void InitControlKeysFromConfig(ScriptSettings settings)
        {
            RegisterControlKey("ToggleModActiveKey", settings.GetValue<String>("CONTROL_KEYBOARD", "ToggleModActiveKey", "None"),
                (Action)ToggleModActiveProc, TriggerCondition.PRESSED);

            RegisterControlKey("ReloadConfigFileKey", settings.GetValue<String>("CONTROL_KEYBOARD", "ReloadConfigFileKey", "None"),
                (Action)ReloadConfigFileProc, TriggerCondition.PRESSED);
            
            RegisterControlKey("MakePedGoRogueKey", settings.GetValue<String>("CONTROL_KEYBOARD", "MakePedGoRogueKey", "None"),
                (Action)delegate { MakePedGoRogueProc(false, false); }, TriggerCondition.PRESSED);

            RegisterControlKey("MakeTargetedPedGoRogueKey", settings.GetValue<String>("CONTROL_KEYBOARD", "MakeTargetedPedGoRogueKey", "None"),
                (Action)delegate { MakePedGoRogueProc(false, true); }, TriggerCondition.PRESSED);

            RegisterControlKey("MakePedPerformActionKey", settings.GetValue<String>("CONTROL_KEYBOARD", "MakePedPerformActionKey", "None"),
                (Action)delegate { MakePedPerformActionProc(); }, TriggerCondition.PRESSED);

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
