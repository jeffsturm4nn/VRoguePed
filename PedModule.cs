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
        private static readonly Vector3 landActWaterReservoirPosition = new Vector3(2150.0f, 5150f, 0f);

        private static WeaponHash RoguePedWeaponHash = WeaponHash.Pistol50;
        private static int RoguePedHealth = 500;

        public static void ProcessPeds()
        {
            try
            {
                string msg = "";

                for (int i = 0; i < Core.RoguePeds.Count; i++)
                {
                    msg += "[" + i + "]: ";

                    RoguePed rp = Core.RoguePeds.ElementAt(i);

                    if (rp == null)
                    {
                        msg += "RoguePed{NULL}\n";
                    }
                    else
                    {
                        msg += rp.ToString() + "\n";
                    }
                }

                Util.Subtitle(msg, 10);

                for (int i = Core.RoguePeds.Count - 1; i >= 0; i--)
                {
                    RoguePed roguePed = Core.RoguePeds[i];

                    if (roguePed != null)
                    {
                        if (!roguePed.IsValid() ||
                            roguePed.Ped.Position.DistanceTo(Game.Player.Character.Position) > 100f)
                        {
                            Core.ProcessedPeds.Remove(roguePed.Ped);
                            Core.RoguePeds.Remove(roguePed);
                        }
                        else if (roguePed.Victims.Count > 0)
                        {
                            Ped victim = roguePed.Victims[0];

                            if (victim == null || !victim.Exists() || victim.IsDead)
                            {
                                roguePed.Victims.Remove(victim);
                                Core.ProcessedPeds.Remove(victim);

                                if (roguePed.Victims.Count == 0)
                                {
                                    Core.ProcessedPeds.Remove(roguePed.Ped);
                                    Core.RoguePeds.Remove(roguePed);
                                }
                            }
                        }
                    }
                }

                
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ProcessPeds() Error:\n " + e.ToString());
            }
        }

        public static void MakePedGoRogueProc()
        {
            try
            {
                Ped roguePed = PedUtil.GetNearestValidRoguePeds(Game.Player.Character, 1, 45f, Core.ProcessedPeds).FirstOrDefault();

                if (roguePed == null || !roguePed.Exists())
                {
                    Util.Notify("VRoguePed Error:\n 'roguePed' not found.");
                    return;
                }

                Ped victimPed = PedUtil.GetNearestValidVictimPeds(roguePed, 1, 45f, Core.ProcessedPeds).FirstOrDefault();

                if (victimPed == null || !victimPed.Exists())
                {
                    Util.Notify("VRoguePed Error:\n 'victimPed' not found.");
                    return;
                }

                roguePed.Weapons.Give(RoguePedWeaponHash, 99, true, true);
                roguePed.MaxHealth = RoguePedHealth;
                roguePed.Health = RoguePedHealth;
                roguePed.RelationshipGroup = (int)Relationship.Companion;

                TaskSequence taskSequence = new TaskSequence();
                VehicleSeat playerVehicleSeat = VehicleSeat.None;

                taskSequence.AddTask.ClearAllImmediately();

                if (roguePed.IsInVehicle())
                {
                    taskSequence.AddTask.LeaveVehicle();

                    if (roguePed.CurrentVehicle == Game.Player.Character.CurrentVehicle)
                    {
                        playerVehicleSeat = VehicleUtil.GetSeatPedIsSittingOn(roguePed, Game.Player.Character.CurrentVehicle);
                    }
                }

                taskSequence.AddTask.RunTo(victimPed.Position, false);
                taskSequence.AddTask.GoTo(victimPed, new Vector3(2.5f, 2.5f, 2.5f), 4000);
                //taskSequence.AddTask.FightAgainst(victimPed, 10000);
                taskSequence.AddTask.AimAt(victimPed, 1100);
                taskSequence.AddTask.ShootAt(victimPed, 7000);

                if (playerVehicleSeat == VehicleSeat.None)
                {
                    playerVehicleSeat = VehicleUtil.GetPlayerVehicleFreeSeat();
                }

                if (playerVehicleSeat != VehicleSeat.None)
                {
                    taskSequence.AddTask.RunTo(Game.Player.Character.CurrentVehicle.Position, false);
                    //taskSequence.AddTask.GoTo(Game.Player.Character.CurrentVehicle, new Vector3(2f, 2f, 2f), 9000);
                    taskSequence.AddTask.EnterVehicle(Game.Player.Character.CurrentVehicle, playerVehicleSeat);
                }
                else
                {
                    Vehicle rogueVehicle = VehicleUtil.GetNearesVehicle(roguePed);

                    if (rogueVehicle != null && rogueVehicle.Exists())
                    {
                        taskSequence.AddTask.RunTo(rogueVehicle.Position, false);
                        taskSequence.AddTask.ClearAllImmediately();
                        taskSequence.AddTask.RunTo(rogueVehicle.Position, false);
                        taskSequence.AddTask.EnterVehicle(rogueVehicle, VehicleSeat.Driver);
                        taskSequence.AddTask.DriveTo(rogueVehicle, landActWaterReservoirPosition, 50f, 90f, (int)DrivingStyle.Rushed);
                    }
                }

                taskSequence.Close();

                roguePed.Task.PerformSequence(taskSequence);
                taskSequence.Dispose();

                RoguePed currentRoguePed = Core.RoguePeds.Where(p => p.Ped.Equals(roguePed)).FirstOrDefault();

                if (currentRoguePed == null || !currentRoguePed.IsValid())
                {
                    Core.RoguePeds.Add(new RoguePed(roguePed, new List<Ped> { victimPed }, playerVehicleSeat));
                    Core.ProcessedPeds.Add(roguePed);
                }
                else
                {
                    currentRoguePed.PlayerVehicleSeat = playerVehicleSeat;
                    currentRoguePed.Victims = new List<Ped> { victimPed };
                }

                //Core.VictimPeds.Add(victimPed);
                Core.ProcessedPeds.Add(victimPed);
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
                RoguePedHealth = settings.GetValue<int>("PED_PARAMETERS", "RoguePedHealth", 300);
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
