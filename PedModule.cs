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

        private static volatile bool hasReachedInterval = false;
        private static int timeCounter = 0;

        public static int FriendlyRoguePedsGroupHash = -1;
        public static int RoguePedTargetsGroupHash = -2;
        public static WeaponHash RoguePedWeaponHash = WeaponHash.Pistol50;
        public static int RoguePedHealth = 500;
        public static int RoguePedClearTasksIntervalInSecs = 300;
        public static int BatchRoguePedCount = 5;
        public static int MaxRoguePedsPerTarget = 2;
        public static float MaxRoguePedRecruitDistance = 40f;
        public static float MaxVictimPedOnFootChaseDistance = 40f;
        public static float MaxVictimPedInVehicleChaseDistance = 40f;
        public static float MaxRoguePedDistanceFromPlayer = 60f;
        //public static float MinVictimPedDistanceToShoot = 19f;
        public static float MaxRoguePedWanderDistance = 30f;
        public static float MaxRoguePedDistanceBeforeDisband = 80f;
        public static bool RoguePedsFollowPlayer = true;

        public static void InitPedRelationshipGroups()
        {
            FriendlyRoguePedsGroupHash = World.AddRelationshipGroup("FriendlyRoguePeds");
            World.SetRelationshipBetweenGroups(Relationship.Companion, FriendlyRoguePedsGroupHash, Game.Player.Character.RelationshipGroup);
        }

        public static void ToggleRoguePedsFollowPlayerProc()
        {
            RoguePedsFollowPlayer = !RoguePedsFollowPlayer;
        }

        public static void KillAllRoguePedsProc()
        {
            for (int i = 0; i < Core.RoguePeds.Count; i++)
            {
                RoguePed roguePed = Core.RoguePeds[i];

                if (roguePed != null && roguePed.IsValid())
                {
                    roguePed.Ped.Kill();
                }
            }
        }

        public static void MakeBatchPedsGoRogueProc()
        {
            for (int i = 0; i < BatchRoguePedCount; i++)
            {
                MakePedGoRogue(false, false);
            }
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
                    bool removeVictimPed = false;

                    if (roguePed != null)
                    {
                        if (!roguePed.IsValid() || roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceBeforeDisband)
                        {
                            if (roguePed.Victim != null)
                            {
                                PedUtil.RemoveVictim(roguePed.Victim);
                            }

                            PedUtil.DeletePedBlip(roguePed.Blip);
                            Core.ProcessedPeds.Remove(roguePed.Ped);

                            PedUtil.DisposePed(roguePed.Ped);
                            Core.RoguePeds.Remove(roguePed);
                        }
                        else if (RoguePedsFollowPlayer)
                        {
                            roguePed.State = RogueState.FOLLOWING_PLAYER;
                        }
                        else if (roguePed.State != RogueState.RUNNING_TOWARDS_PLAYER)
                        {
                            if (roguePed.HasValidVictim())
                            {
                                if (roguePed.Victim.Type == VictimType.NORMAL_PED)
                                {
                                    if ((roguePed.DistanceFromVictim() >= MaxVictimPedOnFootChaseDistance) ||
                                         ((roguePed.Victim.Ped.IsFleeing || roguePed.Victim.Ped.Velocity.Length() > 6f)
                                         && roguePed.DistanceFromVictim() >= MaxVictimPedInVehicleChaseDistance))
                                    {
                                        roguePed.Ped.Task.ClearAll();
                                        removeVictimPed = true;
                                        roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                                    }
                                }
                                else if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                                {
                                    removeVictimPed = true;
                                    roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                                }
                            }
                        }

                        if (removeVictimPed)
                        {
                            PedUtil.RemoveVictim(roguePed.Victim);
                            roguePed.Victim = null;
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
                const int timeInterval = 200 / Constants.UPDATE_INTERVAL;
                bool hasReachedInterval = false;

                if (++timeCounter >= timeInterval)
                {
                    timeCounter = 0;
                    hasReachedInterval = true;
                }

                string sub = "RoguePeds.Count = " + Core.RoguePeds.Count;
                sub += " | ProcPeds.Count = " + Core.ProcessedPeds.Count + "\n";

                for (int i = Core.RoguePeds.Count - 1; i >= 0; i--)
                {
                    RoguePed roguePed = Core.RoguePeds[i];

                    if (roguePed != null && roguePed.IsValid())
                    {
                        //sub += "[i=" + i + ", TS=" + roguePed.Ped.TaskSequenceProgress +
                        //    ", D=" + ((int)roguePed.DistanceFromPlayer()) + "=, S=" + roguePed.State.ToString().Split('_')[0] +
                        //    ", V=" + (!Util.IsValid(roguePed.Victim) ? "None" : ("@ " + ((int)roguePed.DistanceFromVictim()) + "]"));

                        bool hasToClearTasks = false;

                        if (roguePed.Ped.IsInCombat)
                        {
                            roguePed.TargetCombatDuration += Constants.UPDATE_INTERVAL;
                        }

                        if (roguePed.ClearTasksTime > 0)
                        {
                            roguePed.ClearTasksTime -= Constants.UPDATE_INTERVAL;
                        }
                        else
                        {
                            hasToClearTasks = true;
                            roguePed.ClearTasksTime = (RoguePedClearTasksIntervalInSecs * 1000);
                        }

                        if (hasToClearTasks)
                        {
                            if (!roguePed.Ped.IsInVehicle())
                            {
                                Util.Notify("RoguePed[" + i + "] Clearing tasks...");
                                roguePed.Ped.Task.ClearAll();
                            }
                        }
                        else if (roguePed.Ped.TaskSequenceProgress == Constants.TASK_SEQUENCE_IN_PROGRESS)
                        {
                            continue;
                        }
                        else if (roguePed.State == RogueState.LOOKING_FOR_VICTIM)
                        {
                            //sub += "[LOOKING_FOR_VICTIM]";

                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (RoguePedsFollowPlayer)
                            {
                                roguePed.State = RogueState.FOLLOWING_PLAYER;
                            }
                            else if (hasReachedInterval)
                            {
                                roguePed.TargetCombatDuration = 0;

                                if (roguePed.Victim != null)
                                {
                                    PedUtil.RemoveVictim(roguePed.Victim);
                                    roguePed.Victim = null;
                                }

                                VictimPed victim = PedUtil.GetNextVictimPeds(roguePed.Ped, 1, MaxVictimPedOnFootChaseDistance, Core.ProcessedPeds).FirstOrDefault();

                                if (Util.IsValid(victim))
                                {
                                    roguePed.Victim = victim;
                                    roguePed.State = RogueState.ATTACKING_VICTIM;
                                    Core.ProcessedPeds.Add(victim.Ped);
                                }
                                else if (!PedUtil.IsPedWanderingAround(roguePed.Ped))
                                {
                                    roguePed.Ped.Task.WanderAround(Game.Player.Character.Position, MaxRoguePedWanderDistance);
                                }

                                roguePed.Ped.WetnessHeight = 4f;
                            }
                        }
                        /*else if (roguePed.State == RogueState.RUNNING_TOWARDS_VICTIM)
                        {
                            //sub += "[RUNNING_TOWARDS_VICTIM]";

                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (RoguePedsFollowPlayer)
                            {
                                roguePed.State = RogueState.FOLLOWING_PLAYER;
                            }
                            else if (roguePed.HasValidVictim())
                            {
                                if (roguePed.DistanceFromVictim() <= MinVictimPedDistanceToShoot)
                                {
                                    roguePed.State = RogueState.ATTACKING_VICTIM;
                                }
                                else if (hasReachedInterval)
                                {
                                    //if (!roguePed.Ped.IsRunning)
                                    {
                                        roguePed.Ped.Task.RunTo(roguePed.Victim.Ped.Position, false);
                                    }
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }*/
                        else if (roguePed.State == RogueState.ATTACKING_VICTIM)
                        {
                            sub += "[ATTACKING_VICTIM]";

                            if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                roguePed.State = RogueState.RUNNING_TOWARDS_PLAYER;
                            }
                            else if (RoguePedsFollowPlayer)
                            {
                                roguePed.State = RogueState.FOLLOWING_PLAYER;
                            }
                            else if (roguePed.HasValidVictim())
                            {
                                if (hasReachedInterval)
                                {
                                    //if (!roguePed.Victim.Ped.IsInVehicle())
                                    //{
                                        //if (!roguePed.IsInCombatWithVictim())
                                        {
                                            roguePed.Ped.Task.FightAgainst(roguePed.Victim.Ped, -1);
                                        }
                                    //}
                                    //else
                                    //{
                                        //if (roguePed.TargetCombatDuration <= 4000)
                                        //{
                                        //    roguePed.Ped.Task.ShootAt(roguePed.Victim.Ped, 4000, FiringPattern.FullAuto);
                                        //}
                                        //else if (!roguePed.IsInCombatWithVictim())
                                    //    {
                                    //        roguePed.Ped.Task.ClearAll();
                                    //        roguePed.Ped.Task.FightAgainst(roguePed.Victim.Ped, -1);
                                    //    }
                                    //}
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                        else if (roguePed.State == RogueState.RUNNING_TOWARDS_PLAYER)
                        {
                            sub += "[RUNNING_TOWARDS_PLAYER]";

                            if (RoguePedsFollowPlayer)
                            {
                                roguePed.State = RogueState.FOLLOWING_PLAYER;
                            }
                            else if (roguePed.DistanceFromPlayer() >= MaxRoguePedDistanceFromPlayer)
                            {
                                if (hasReachedInterval)
                                {
                                    roguePed.Ped.Task.RunTo(Game.Player.Character.Position, false);
                                }
                            }
                            else
                            {
                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }
                        else if (roguePed.State == RogueState.FOLLOWING_PLAYER)
                        {
                            //sub += "[FOLLOWING_PLAYER]";

                            if (RoguePedsFollowPlayer)
                            {
                                if (hasReachedInterval)
                                {
                                    Vector3 offsetFromPlayer = new Vector3(0f, 2f, 0f);

                                    if (Game.Player.Character.IsRunning || Game.Player.Character.IsSprinting || roguePed.DistanceFromPlayer() > 12f)
                                    {
                                        roguePed.Ped.Task.RunTo((Game.Player.Character.Position + offsetFromPlayer), false);
                                    }
                                    else if (!Game.Player.Character.IsInVehicle())
                                    {
                                        if (!roguePed.Ped.IsInVehicle())
                                        {
                                            if (roguePed.DistanceFromPlayer() > 4f)
                                            {
                                                roguePed.Ped.Task.GoTo(Game.Player.Character, offsetFromPlayer, 1000);
                                            }
                                        }
                                        else
                                        {
                                            PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.LeaveVehicle(); });
                                        }
                                    }
                                    else if (!roguePed.Ped.IsInVehicle(Game.Player.Character.CurrentVehicle))
                                    {
                                        roguePed.PlayerVehicleSeat = VehicleUtil.GetPlayerVehicleFreeSeat();

                                        if (roguePed.PlayerVehicleSeat != VehicleSeat.None)
                                        {
                                            PedUtil.PerformTaskSequence(roguePed.Ped, ts =>
                                            {
                                                ts.AddTask.ClearAll();
                                                ts.AddTask.EnterVehicle(Game.Player.Character.CurrentVehicle, roguePed.PlayerVehicleSeat, -1, 100f);
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (roguePed.Ped.IsInVehicle())
                                {
                                    PedUtil.PerformTaskSequence(roguePed.Ped, ts => { ts.AddTask.LeaveVehicle(); });
                                }

                                roguePed.State = RogueState.LOOKING_FOR_VICTIM;
                            }
                        }

                        //sub += "\n";
                        //sub += " Spd(" + roguePed.Ped.Velocity.Length() + ")\n";
                    }
                }

                Util.Subtitle(sub);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed UpdateRoguePedsState() Error:\n " + e.ToString());
            }
        }

        public static void MakePedGoRogueProc(bool targetedVictimPed, bool targetedRoguePed)
        {
            MakePedGoRogue(targetedVictimPed, targetedRoguePed);
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
                }
                else
                {
                    roguePed = PedUtil.GetNearestValidRoguePeds(Game.Player.Character, 1, MaxRoguePedRecruitDistance, Core.ProcessedPeds).FirstOrDefault();
                }

            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed RecruitRoguePed() Error:\n" + e.ToString(), true);
            }

            return roguePed;
        }

        public static void MakePedGoRogue(bool targetedVictimPed, bool targetedRoguePed)
        {
            try
            {
                Ped roguePed = RecruitRoguePed(targetedRoguePed);

                if (roguePed == null || !roguePed.Exists())
                {
                    return;
                }

                VictimPed victimPed = null;

                if (targetedVictimPed && Game.Player.IsAlive && Game.Player.IsAiming)
                {
                    Entity targetEntity = null;
                    Util.GetEntityPlayerIsAimingAt(ref targetEntity);

                    if (targetEntity != null && targetEntity.Exists() && Util.IsNPCPed(targetEntity)
                        && !Core.ProcessedPeds.Contains((Ped)targetEntity))
                    {
                        victimPed = new VictimPed(((Ped)targetEntity), VictimType.PLAYER_TARGET);
                    }
                }

                roguePed.Task.ClearAll();

                PedUtil.SetRoguePedParameters(roguePed);
                PedUtil.MakePedCombatResilient(roguePed);

                VehicleSeat playerVehicleSeat = VehicleSeat.None;

                if (roguePed.IsInVehicle())
                {
                    if (roguePed.CurrentVehicle == Game.Player.Character.CurrentVehicle)
                    {
                        playerVehicleSeat = VehicleUtil.GetSeatPedIsSittingOn(roguePed, Game.Player.Character.CurrentVehicle);
                    }
                }

                if (playerVehicleSeat == VehicleSeat.None)
                {
                    playerVehicleSeat = VehicleUtil.GetPlayerVehicleFreeSeat();
                }

                RoguePed currentRoguePed = Core.RoguePeds.Where(p => p.Ped.Equals(roguePed)).FirstOrDefault();

                if (currentRoguePed == null || !currentRoguePed.IsValid())
                {
                    Blip blip = PedUtil.AttachBlipToPed(roguePed, BlipColor.BlueLight, (Core.RoguePeds.Count + 1));
                    Core.RoguePeds.Add(new RoguePed(roguePed, victimPed, playerVehicleSeat, blip, (RoguePedClearTasksIntervalInSecs * 1000)));
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
                    Core.ProcessedPeds.Add(victimPed.Ped);
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
                RoguePedHealth = settings.GetValue<int>("PED_PARAMETERS", "RoguePedHealth", 1000);
                String weaponName = settings.GetValue("PED_PARAMETERS", "RoguePedWeaponName", "Pistol50");

                if (weaponName == null || weaponName.Length == 0 ||
                    weaponName.Equals("None", StringComparison.InvariantCultureIgnoreCase))
                {
                    weaponName = "Unarmed";
                }

                RoguePedWeaponHash = (WeaponHash)Enum.Parse(typeof(WeaponHash), weaponName);

                MaxRoguePedRecruitDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedRecruitDistance", 40);
                MaxVictimPedOnFootChaseDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxVictimPedOnFootChaseDistance", 40);
                MaxVictimPedInVehicleChaseDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxVictimPedInVehicleChaseDistance", 40);
                MaxRoguePedDistanceFromPlayer = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedDistanceFromPlayer", 60);
                //MinVictimPedDistanceToShoot = settings.GetValue<int>("PED_PARAMETERS", "MinVictimPedDistanceToShoot", 19);
                MaxRoguePedWanderDistance = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedWanderDistance", 30);
                MaxRoguePedDistanceBeforeDisband = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedDistanceBeforeDisband", 80);
                RoguePedClearTasksIntervalInSecs = settings.GetValue<int>("PED_PARAMETERS", "RoguePedLifetimeInSeconds", 300);
                BatchRoguePedCount = settings.GetValue<int>("PED_PARAMETERS", "BatchRoguePedCount", 5);
                MaxRoguePedsPerTarget = settings.GetValue<int>("PED_PARAMETERS", "MaxRoguePedsPerTarget", 2);
            }
            catch (Exception e)
            {
                Util.Notify("VRoguePed ReadPedParamsFromConfig() Error:\n " + e.ToString());
            }
        }
    }
}
