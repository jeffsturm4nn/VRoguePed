using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VRoguePed
{
    internal class PedModule
    {
        private static readonly Vector3 airportEntracePosition = new Vector3(-1337.0f, -3044.0f, 13.9f);

        private static WeaponHash RoguePedWeaponHash = WeaponHash.Pistol50;

        public static void MakePedGoRogueProc()
        {
            try
            {
                Ped roguePed = World.GetNearbyPeds(Game.Player.Character, 45f)
                     .Where(p => p != null
                        && p.Exists()
                        && p != Game.Player.Character
                        && !p.IsRagdoll
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

                Ped victimPed = World.GetNearbyPeds(roguePed, 45f)
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

                roguePed.Weapons.Give(RoguePedWeaponHash, 99, true, true);
                roguePed.MaxHealth = 300;
                roguePed.Health = 300;
                roguePed.RelationshipGroup = (int)Relationship.Companion;

                TaskSequence taskSequence = new TaskSequence();

                taskSequence.AddTask.ClearAll();
                taskSequence.AddTask.RunTo(victimPed.Position, false);
                //taskSequence.AddTask.StandStill(3000);

                taskSequence.AddTask.FightAgainst(victimPed);

                //taskSequence.AddTask.AimAt(victimPed, 1000);
                //taskSequence.AddTask.ShootAt(victimPed, 6000);

                Vehicle rogueVehicle = VehicleUtil.GetNearesVehicle(roguePed);

                if (rogueVehicle != null && rogueVehicle.Exists() && rogueVehicle.IsDriveable)
                {
                    taskSequence.AddTask.RunTo(rogueVehicle.Position, false);
                    taskSequence.AddTask.EnterVehicle(rogueVehicle, VehicleSeat.Driver);
                    taskSequence.AddTask.DriveTo(rogueVehicle, airportEntracePosition, 50f, 15f, (int)DrivingStyle.Rushed);
                }
                taskSequence.Close();

                roguePed.Task.PerformSequence(taskSequence);
                taskSequence.Dispose();
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed MakePedGoRogueProc() Error:\n" + e.ToString(), true);
            }
        }

        public static void ReadPedParamsFromConfig(ScriptSettings settings)
        {
            try
            {
                String weaponName = settings.GetValue("PED_PARAMETERS", "RoguePedWeaponName", "Pistol50");

                if (weaponName == null || weaponName.Length == 0 ||
                    weaponName.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                {
                    weaponName = "Unarmed";
                }

                RoguePedWeaponHash = (WeaponHash)Enum.Parse(typeof(WeaponHash), weaponName);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ReadPedParamsFromConfig() Error:\n " + e.ToString());
                RoguePedWeaponHash = WeaponHash.Pistol50;
            }
        }
    }
}
