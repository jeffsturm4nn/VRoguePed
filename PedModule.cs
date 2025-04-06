using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms.VisualStyles;

namespace VRoguePed
{
    internal class PedModule
    {
        private static readonly Vector3 AirportEntracePosition = new Vector3(-1337.0f, -3044.0f, 13.9f);
        private static readonly Vector3 LandActWaterReservoirPosition = new Vector3(2150.0f, 5150f, 0f);
        private static readonly Vector3 FranklingHouse1Position = new Vector3(-14f, -1458f, 30f);

        private static int FriendlyRoguePedsGroupHash = -1;
        private static WeaponHash RoguePedWeaponHash = WeaponHash.Pistol50;
        private static int RoguePedHealth = 500;
        private static Timer Timer = new Timer(1000);

        private static volatile bool HasReachedInterval = false;

        private static void OnTimeElapsed(Object source, ElapsedEventArgs e)
        {
            HasReachedInterval = true;
        }
        public static void InitPedRelationshipGroups()
        {
            FriendlyRoguePedsGroupHash = World.AddRelationshipGroup("FriendlyRoguePeds");
            World.SetRelationshipBetweenGroups(Relationship.Respect, FriendlyRoguePedsGroupHash, Game.Player.Character.RelationshipGroup);
        }

        public static void InitGlobalTimer()
        {
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Timer.Elapsed += OnTimeElapsed;
        }

        public static void AddPedToFriendlyGroup(Ped ped)
        {
            ped.RelationshipGroup = FriendlyRoguePedsGroupHash;
        }

        public static void CheckValidRoguePeds()
        {
            try
            {
                for (int i = Core.RoguePeds.Count - 1; i >= 0; i--)
                {
                    RoguePed roguePed = Core.RoguePeds[i];

                    if (roguePed != null)
                    {
                        if (!roguePed.IsValid() ||
                            roguePed.Ped.Position.DistanceTo(Game.Player.Character.Position) > 100f)
                        {
                            Core.RoguePeds.Remove(roguePed);
                            Core.ProcessedPeds.Remove(roguePed.Ped);
                            Core.ProcessedPeds.Remove(roguePed.Victim);
                        }
                        else if (roguePed.State != RogueState.RUNNING_TOWARDS_PLAYER)
                        {
                            if (!Util.IsValid(roguePed.Victim) ||
                                (roguePed.Victim.IsDead || roguePed.DistanceFromVictim() > 35f))
                            {
                                Core.ProcessedPeds.Remove(roguePed.Victim);

                                roguePed.Victim = null;
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                    }
                    else
                    {
                        Core.RoguePeds.RemoveAt(i);
                    }
                }
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ProcessPeds() Error:\n " + e.ToString());
            }
        }

        public static void UpdateRoguePedsState()
        {
            try
            {
                string sub = "RoguePeds.Count = " + Core.RoguePeds.Count;
                sub += "\nProcessedPeds.Count = " + Core.ProcessedPeds.Count;

                for (int i = Core.RoguePeds.Count - 1; i >= 0; i--)
                {
                    RoguePed roguePed = Core.RoguePeds[i];

                    if (roguePed != null && roguePed.IsValid())
                    {
                        sub += "\nroguePed[" + i + "]: TaskSeqProg(" + roguePed.Ped.TaskSequenceProgress +
                            "), Dist(" + roguePed.DistanceFromPlayer() +
                            "), State(" + roguePed.State.ToString() +
                            "), Victim(" + (!Util.IsValid(roguePed.Victim) ? "None)" :
                            ("Found, Dist: " + roguePed.DistanceFromVictim() +
                            //"), Alive(" + roguePed.Victim.IsAlive + "), Dead(" + roguePed.Victim.IsDead +
                            "), HVV(" + roguePed.HasValidVictim() + ") "));

                        if (roguePed.Ped.TaskSequenceProgress == Constants.TASK_SEQUENCE_IN_PROGRESS)
                        {
                            continue;
                        }
                        else if (roguePed.State == RogueState.LOOKING_FOR_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= 60f)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (HasReachedInterval)
                            {
                                if (Util.IsValid(roguePed.Victim))
                                {
                                    Core.ProcessedPeds.Remove(roguePed.Victim);
                                    roguePed.Victim = null;
                                }

                                Ped victim = PedUtil.GetNearestValidVictimPeds(roguePed.Ped, 1, 30f, Core.ProcessedPeds).FirstOrDefault();

                                if (Util.IsValid(victim))
                                {
                                    roguePed.Victim = victim;
                                    roguePed.State = RogueState.RUNNING_TOWARDS_VICTIM;
                                }
                                else if(!roguePed.Ped.IsWalking)
                                {
                                    roguePed.Ped.Task.WanderAround(Game.Player.Character.Position, 60f);
                                }

                                HasReachedInterval = false;
                            }
                        }
                        else if (roguePed.State == RogueState.RUNNING_TOWARDS_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= 70f)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (roguePed.DistanceFromVictim() <= 10f)
                            {
                                roguePed.State = RogueState.SHOOTING_AT_VICTIM;
                            }
                            else if (HasReachedInterval)
                            {
                                roguePed.Ped.Task.RunTo(roguePed.Victim.Position, false);
                                HasReachedInterval = false;
                            }
                        }
                        else if (roguePed.State == RogueState.SHOOTING_AT_VICTIM)
                        {
                            if (roguePed.DistanceFromPlayer() >= 70f)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (roguePed.HasValidVictim())
                            {
                                if (!roguePed.Ped.IsShooting)
                                {
                                    if (roguePed.DistanceFromVictim() <= 10f)
                                    {
                                        if (HasReachedInterval)
                                        {
                                            PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.ShootAt(roguePed.Victim, 5000, FiringPattern.FullAuto); });
                                            HasReachedInterval = false;
                                        }
                                    }
                                    else
                                    {
                                        roguePed.State = RogueState.RUNNING_TOWARDS_VICTIM;
                                    }
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                        else if (roguePed.State == RogueState.RUNNING_TOWARDS_PLAYER)
                        {
                            if (roguePed.DistanceFromPlayer() >= 70f)
                            {
                                if (HasReachedInterval)
                                {
                                    roguePed.Ped.Task.RunTo(Game.Player.Character.Position, false);
                                    HasReachedInterval = false;
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                    }
                }

                UI.ShowSubtitle(sub);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed UpdateRoguePedsState() Error:\n " + e.ToString());
            }
        }

        public static void MakePedGoRogueProc(bool findVictimPed)
        {
            MakePedGoRogue(findVictimPed);
        }

        private static Ped RecruitRoguePed(bool targetedRoguePed)
        {
            Ped roguePed = null;

            try
            {
                if (targetedRoguePed && Game.Player.IsAlive && Game.Player.IsAiming)
                {
                    Entity targetEntity = null;
                    Util.GetEntityPlayerIsAimingAt(ref targetEntity);

                    if (targetEntity != null && targetEntity.Exists() && Util.IsNPCPed(targetEntity)
                        && !Core.ProcessedPeds.Contains((Ped)targetEntity))
                    {
                        roguePed = (Ped)targetEntity;
                    }

                    UI.Notify("MakePedGoRogue(false): roguePed = " + roguePed != null ? roguePed.ToString() : "NULL");
                }
                else
                {
                    roguePed = PedUtil.GetNearestValidRoguePeds(Game.Player.Character, 1, 40f, Core.ProcessedPeds).FirstOrDefault();
                }

            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed RecruitRoguePed() Error:\n" + e.ToString(), true);
            }

            return roguePed;
        }

        public static void MakePedGoRogue(bool findVictimPed)
        {
            try
            {
                Ped roguePed = RecruitRoguePed(false);

                if (roguePed == null || !roguePed.Exists())
                {
                    Util.Notify("VRoguePed Error:\n 'roguePed' not found.");
                    return;
                }

                Ped victimPed = null;

                if (findVictimPed && Game.Player.IsAlive && Game.Player.IsAiming)
                {
                    Entity targetEntity = null;
                    Util.GetEntityPlayerIsAimingAt(ref targetEntity);

                    if (targetEntity != null && targetEntity.Exists() && Util.IsNPCPed(targetEntity)
                        && !Core.ProcessedPeds.Contains((Ped)targetEntity))
                    {
                        victimPed = (Ped)targetEntity;
                    }

                    UI.Notify("MakePedGoRogue(true): victimPed = " + victimPed != null ? victimPed.ToString() : "NULL");
                }
                else
                {
                    victimPed = PedUtil.GetNearestValidVictimPeds(roguePed, 1, 45f, Core.ProcessedPeds).FirstOrDefault();
                }

                if (victimPed == null || !victimPed.Exists())
                {
                    //Util.Notify("VRoguePed Error:\n 'victimPed' not found.");
                    //return;
                }
                else if (victimPed == roguePed)
                {
                    Util.Notify("VRoguePed Error:\n 'victimPed' and 'roguePed' are the same Ped.");
                    return;
                }

                roguePed.Weapons.Give(RoguePedWeaponHash, 999, true, true);
                roguePed.MaxHealth = RoguePedHealth;
                roguePed.Health = RoguePedHealth;
                roguePed.CanRagdoll = false;
                roguePed.CanSufferCriticalHits = false;
                roguePed.CanWrithe = false;
                roguePed.MaxSpeed = 50f;
                roguePed.WetnessHeight = 100;

                //roguePed.RelationshipGroup = (int)Relationship.Companion;

                VehicleSeat playerVehicleSeat = VehicleSeat.None;
                //TaskSequence taskSequence = new TaskSequence();

                //taskSequence.AddTask.ClearAllImmediately();

                if (roguePed.IsInVehicle())
                {
                    //LeaveVehicleFlags leaveVehicleFlags = LeaveVehicleFlags.None;

                    //if (roguePed.CurrentVehicle != Game.Player.Character.CurrentVehicle)
                    //{
                    //    leaveVehicleFlags = LeaveVehicleFlags.LeaveDoorOpen;
                    //}

                    //taskSequence.AddTask.LeaveVehicle(leaveVehicleFlags);

                    if (roguePed.CurrentVehicle == Game.Player.Character.CurrentVehicle)
                    {
                        playerVehicleSeat = VehicleUtil.GetSeatPedIsSittingOn(roguePed, Game.Player.Character.CurrentVehicle);
                    }
                }

                //taskSequence.AddTask.RunTo(victimPed.Position, false);
                //taskSequence.AddTask.GoTo(victimPed, new Vector3(3f, 3f, 3f), 4000);
                //taskSequence.AddTask.FightAgainst(victimPed, 10000);
                //taskSequence.AddTask.AimAt(victimPed, 1000);
                //taskSequence.AddTask.ShootAt(victimPed, 8000, FiringPattern.FullAuto);

                if (playerVehicleSeat == VehicleSeat.None)
                {
                    playerVehicleSeat = VehicleUtil.GetPlayerVehicleFreeSeat();
                }

                //if (playerVehicleSeat != VehicleSeat.None)
                //{
                //    taskSequence.AddTask.RunTo(Game.Player.Character.CurrentVehicle.Position, false);
                //    //taskSequence.AddTask.GoTo(Game.Player.Character.CurrentVehicle, new Vector3(2f, 2f, 2f), 7000);
                //    taskSequence.AddTask.EnterVehicle(Game.Player.Character.CurrentVehicle, playerVehicleSeat);
                //}
                //else
                //{
                //    Vehicle rogueVehicle = VehicleUtil.GetNearesVehicle(roguePed);

                //    if (rogueVehicle != null && rogueVehicle.Exists())
                //    {
                //        taskSequence.AddTask.RunTo(rogueVehicle.Position, false);
                //        //taskSequence.AddTask.GoTo(rogueVehicle);
                //        //taskSequence.AddTask.EnterVehicle(rogueVehicle, VehicleSeat.Driver);
                //        taskSequence.AddTask.DriveTo(rogueVehicle, FranklingHouse1Position, 20f, 90f, (int)DrivingStyle.Rushed);
                //        taskSequence.AddTask.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                //        taskSequence.AddTask.RunTo(LandActWaterReservoirPosition);
                //    }
                //}

                //taskSequence.Close();

                //roguePed.Task.PerformSequence(taskSequence);
                //taskSequence.Dispose();

                RoguePed currentRoguePed = Core.RoguePeds.Where(p => p.Ped.Equals(roguePed)).FirstOrDefault();

                if (currentRoguePed == null || !currentRoguePed.IsValid())
                {
                    Core.RoguePeds.Add(new RoguePed(roguePed, victimPed, playerVehicleSeat));
                    Core.ProcessedPeds.Add(roguePed);
                    AddPedToFriendlyGroup(roguePed);
                }
                else
                {
                    currentRoguePed.PlayerVehicleSeat = playerVehicleSeat;
                    currentRoguePed.Victim = victimPed;
                }


                if (Util.IsValid(victimPed))
                {
                    Core.ProcessedPeds.Add(victimPed); 
                }
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
