using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using GTA;

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
            Core.ProcessConfigFile();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!ModActive)
            {
                Script.Wait(1);
                return;
            }

            
        }
    }
}
