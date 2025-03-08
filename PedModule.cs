using GTA;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRoguePed
{
    internal class PedModule
    {
        private static Ped roguePed;
        private static Ped victimPed;
        private static Blip rogueBlip;
        private static Blip victimBlip;
        private static Vehicle targetVehicle;
        private static TaskSequence taskSequence;
        private static Vector3 airportEntracePosition = new Vector3(-1337.0f, -3044.0f, 13.9f);

        public static void MakePedGoRogueProc()
        {
            try
            {
                roguePed = World.GetNearbyPeds(Game.Player.Character, 40f)
                     .Where(p => p != null
                        && p.Exists()
                        && p != Game.Player.Character
                        //&& !p.IsRagdoll
                        && p.IsAlive
                        && p.IsHuman
                        && p.IsOnFoot)
                    .OrderBy(p => Math.Abs(p.Position.DistanceTo(Game.Player.Character.Position)))
                    .FirstOrDefault();

                if (roguePed == null || !roguePed.Exists())
                {
                    Util.Notify("VRoguePed Error:\n 'roguePed' is invalid.");
                    return;
                }

                victimPed = World.GetNearbyPeds(roguePed, 40f)
                    .Where(p => p != null
                    && p.Exists()
                    && p != roguePed
                    && p != Game.Player.Character
                    //&& !p.IsRagdoll
                    && p.IsAlive
                    && p.IsHuman
                    && p.IsOnFoot)
                    .OrderBy(p => Math.Abs(p.Position.DistanceTo(roguePed.Position)))
                    .FirstOrDefault();

                if (victimPed == null || !victimPed.Exists())
                {
                    Util.Notify("VRoguePed Error:\n 'victimPed' is invalid.");
                    return;
                }

                roguePed.Weapons.Give(GTA.Native.WeaponHash.Pistol50, 99, true, true);

                //rogueBlip = roguePed.AddBlip();
                //rogueBlip.IsFlashing = true;
                //rogueBlip.Color = BlipColor.Green;
                //rogueBlip.CategoryType = BlipCategoryType.DistanceShown;
                //rogueBlip.DisplayType = BlipDisplayType.BothMapNoSelectable;
                //rogueBlip.NumberLabel = 1;
                //rogueBlip.Name = "RoguePed";

                //victimBlip = victimPed.AddBlip();
                //victimBlip.IsFlashing = true;
                //victimBlip.Color = BlipColor.Blue;
                //victimBlip.CategoryType = BlipCategoryType.DistanceShown;
                //victimBlip.DisplayType = BlipDisplayType.BothMapNoSelectable;
                //victimBlip.NumberLabel = 2;
                //victimBlip.Name = "VictimPed";

                taskSequence = new TaskSequence();

                taskSequence.AddTask.ClearAll();
                taskSequence.AddTask.LookAt(victimPed, 4000);
                taskSequence.AddTask.RunTo(victimPed.Position, false);
                taskSequence.AddTask.AimAt(victimPed, 1000);
                taskSequence.AddTask.ShootAt(victimPed, 5000);
                taskSequence.AddTask.EnterVehicle();
                //taskSequence.AddTask.DriveTo(roguePed.CurrentVehicle, airportEntracePosition, 30f, VehicleDrivingFlags.DrivingModePloughThrough, 50f);
                //taskSequence.Close();

                roguePed.Task.PerformSequence(taskSequence);

                Util.Subtitle("MakePedGoRogueProc() called.", 1700);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed MakePedGoRogueProc() Error:\n" + e.ToString(), true);
            }
        }
    }
}
