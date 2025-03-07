using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using GTA;

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
                Util.Notify("VRoguePed Init Error:\n " + e.Message, true);
            }
        }

        private void InitializeScript()
        {

        }

        private void OnTick(object sender, EventArgs e)
        {
            var pedsNearbyPlayer = PedUtil.GetNearestValidPeds(Game.Player.Character, 1);

            if (pedsNearbyPlayer.Count > 0)
            {
                var targetPed = pedsNearbyPlayer[0];
                var nearbyPeds = PedUtil.GetNearestValidPeds(targetPed, 5); 
            } 
           
        }
    }
}
