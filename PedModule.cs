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
        private static Vector3 airportEntracePosition = new Vector3(-1337.0f, -3044.0f, 13.9f);

        public static void MakePedGoRogueProc()
        {
            try
            {
                Ped roguePed = World.GetNearbyPeds(Game.Player.Character, 40f)
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

                Ped victimPed = World.GetNearbyPeds(roguePed, 40f)
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

                TaskSequence taskSequence = new TaskSequence();

                taskSequence.AddTask.ClearAllImmediately();
                taskSequence.AddTask.LookAt(victimPed, 5000);
                taskSequence.AddTask.RunTo(victimPed.Position, false);
                taskSequence.AddTask.StandStill(3000);
                taskSequence.AddTask.AimAt(victimPed, 1000);
                taskSequence.AddTask.ShootAt(victimPed, 6000);

                Vehicle rogueVehicle = VehicleUtil.GetNearesVehicle(roguePed);

                if (rogueVehicle != null && rogueVehicle.Exists() && rogueVehicle.IsDriveable)
                {
                    taskSequence.AddTask.RunTo(rogueVehicle.Position, false);
                    taskSequence.AddTask.EnterVehicle(rogueVehicle, VehicleSeat.Driver);
                    taskSequence.AddTask.DriveTo(rogueVehicle, airportEntracePosition, 30f, 15f, (int)DrivingStyle.Rushed);
                }
                taskSequence.Close();

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
