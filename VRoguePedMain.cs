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

            //var pedsNearbyPlayer = new List<Ped>();
            //pedsNearbyPlayer.Add(World.GetClosestPed(Game.Player.Character.Position, 20.0f));

            if (pedsNearbyPlayer.Count > 0)
            {
                var targetPed = pedsNearbyPlayer[0];

                var nearbyPeds = PedUtil.GetNearestValidPeds(targetPed, 1);

                //var nearbyPeds = new List<Ped>();
                //nearbyPeds.Add(World.GetClosestPed(targetPed.Position, 20.0f));

                if (nearbyPeds.Count > 0)
                {
                    Ped ped = nearbyPeds[0];
                    TaskSequence ts = new TaskSequence();

                    targetPed.Weapons.Give(WeaponHash.Pistol50, 999, true, true);

                    ts.AddTask.LookAt(ped.Position, 4000, LookAtFlags.FastTurnRate, LookAtPriority.VeryHigh);
                    ts.AddTask.RunTo(ped.Position);

                    //if (ped.IsInVehicle() && (ped.CurrentVehicle.IsStopped || ped.CurrentVehicle.IsStoppedAtTrafficLights))
                    //{
                    //    ts.AddTask.OpenVehicleDoor(ped.CurrentVehicle, VehicleSeat.Driver, -1, PedMoveBlendRatio.Run);
                    //}
                    ts.AddTask.AimGunAtEntity(ped, 1000);
                    ts.AddTask.ShootAt(ped, 6000);


                    targetPed.Task.PerformSequence(ts);
                }
                else
                {
                    Util.Subtitle("Couldn't find a Ped near the target.");
                }
            }
            else
            {
                Util.Subtitle("Couldn't find a Ped near the player.");
            }
        }
    }
}
