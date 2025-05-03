using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

using GTA;
using GTA.Math;
using static VRoguePed.Core;

namespace VRoguePed
{
    public class VRoguePedMain : Script
    {
        public VRoguePedMain()
        {
            try
            {
                this.Tick += OnTick;
                this.KeyDown += OnKeyDown;
                this.Interval = Constants.UPDATE_INTERVAL;

                InitializeScript();

                Util.Notify("VRoguePed vX.Y test-build", false);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed Init Error:\n " + e.ToString(), true);
            }
        }

        private void InitializeScript()
        {
            Core.ProcessConfigFile(false);
            PedModule.InitPedRelationshipGroups();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!ModActive)
            {
                Script.Wait(1);
                return;
            }

            InputModule.CheckForKeysHeldDown();
            InputModule.CheckForKeysReleased();

            PedModule.CheckValidRoguePeds();
            PedModule.UpdateRoguePedsState();
        }

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                for (int i = 0; i < ControlKeys.Count; i++)
                {
                    if (!ModActive)
                    {
                        if (ControlKeys[i].name == "ToggleModActiveKey" && Keyboard.IsKeyListPressed(ControlKeys[i].keys))
                        {
                            ControlKeys[i].callback.Invoke();
                            ControlKeys[i].wasPressed = true;
                            break;
                        }
                    }
                    else
                    {
                        if (ControlKeys[i].condition.HasFlag(TriggerCondition.PRESSED) && Keyboard.IsKeyListPressed(ControlKeys[i].keys))
                        {
                            if (!ControlKeys[i].wasPressed)
                            {
                                ControlKeys[i].callback.Invoke();
                                ControlKeys[i].wasPressed = true;
                            }

                            break;
                        }
                        else if (ControlKeys[i].condition.HasFlag(TriggerCondition.HELD) && Keyboard.IsKeyListPressed(ControlKeys[i].keys))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Util.Notify("VRoguePed OnKeyDown Error:\n" + exc.ToString(), false);
            }
        }
    }
}
